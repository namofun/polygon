using Microsoft.AspNetCore.Mvc;
using Polygon.Entities;
using Polygon.Storages;
using SatelliteSite.PolygonModule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace SatelliteSite.PolygonModule.Controllers
{
    [Area("Polygon")]
    [Route("[area]/{pid}/[controller]")]
    public class SubmissionsController : PolygonControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> List(int pid, bool all = false, int page = 1)
        {
            if (page < 0) return NotFound();

            Expression<Func<Submission, bool>> cond = s => s.ProblemId == pid;
            if (!all) cond = cond.Combine(s => s.ExpectedResult != null);

            var result = await Facade.Submissions.ListWithJudgingAsync((page, 30), cond, true);

            if (result.Any())
            {
                var (id1, id2) = (result[0].SubmissionId, result[^1].SubmissionId);
                if (id1 > id2) (id1, id2) = (id2, id1);
                var cond2 = cond.Combine(s => s.Id >= id1 && s.Id <= id2);
                var names = await Facade.Submissions.GetAuthorNamesAsync(cond2);
                foreach (var item in result)
                    item.AuthorName = names.GetValueOrDefault(item.SubmissionId, "SYSTEM");
            }

            ViewBag.All = all;
            ViewBag.Testcase = await Facade.Testcases.ListAsync(pid);
            return View(result);
        }


        [HttpGet("{sid}")]
        public async Task<IActionResult> Detail(int pid, int sid, int? jid)
        {
            var s = await Facade.Submissions.FindAsync(sid, true);
            if (s == null || s.ProblemId != pid) return NotFound();
            var j = s.Judgings.SingleOrDefault(jj => jid.HasValue ? jj.Id == jid : jj.Active);
            if (j == null) return NotFound();
            var l = await Facade.Languages.FindAsync(s.Language);
            var det = await Facade.Judgings.GetDetailsAsync(pid, j.Id);
            var uname = await Facade.Submissions.GetAuthorNameAsync(sid);

            return View(new SolutionV2
            {
                SubmissionId = s.Id,
                ContestId = s.ContestId,
                JudgingId = j.Id,
                Language = s.Language,
                TeamId = s.TeamId,
                Verdict = j.Status,
                ExpectedVerdict = s.ExpectedResult,
                SourceCode = s.SourceCode,
                CompileError = j.CompileError,
                AllJudgings = s.Judgings,
                TestcaseNumber = det.Count(),
                LanguageName = l.Name,
                TimeFactor = l.TimeFactor,
                AuthorName = uname ?? "SYSTEM",
                ServerName = j.Server ?? "UNKNOWN",
                ExecutionMemory = j.ExecuteMemory,
                ExecutionTime = j.ExecuteTime,
                CodeLength = s.CodeLength,
                Ip = s.Ip,
                ProblemId = Problem.Id,
                Skipped = s.Ignored,
                TotalScore = j.TotalScore,
                Judging = j,
                Time = s.Time,
                CombinedRunCompare = Problem.CombinedRunCompare,
                TimeLimit = Problem.TimeLimit,
                DetailsV2 = det,
                LanguageFileExtension = l.FileExtension,
            });
        }


        [HttpGet("[action]")]
        [ValidateAjaxWindow]
        public async Task<IActionResult> Submit()
        {
            ViewBag.Language = await Facade.Languages.ListAsync();
            return Window(new CodeSubmitModel());
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(CodeSubmitModel model)
        {
            var lang = await Facade.Languages.FindAsync(model.Language);
            if (lang == null) return BadRequest();

            var sub = await Facade.Submissions.CreateAsync(
                code: model.Code,
                language: lang.Id,
                problemId: Problem.Id,
                contestId: null,
                teamId: int.Parse(User.GetUserId()),
                ipAddr: HttpContext.Connection.RemoteIpAddress,
                via: "polygon-page",
                username: User.GetUserName(),
                expected: Verdict.Unknown);

            return RedirectToAction(nameof(Detail), new { sid = sub.Id });
        }


        [HttpGet("{sid}/[action]")]
        public async Task<IActionResult> RejudgeOne(int pid, int sid)
        {
            var sub = await Facade.Submissions.FindAsync(sid);
            if (sub == null || sub.ProblemId != pid)
                return NotFound();

            if (sub.ContestId != 0)
                StatusMessage = "Error contest submissions should be rejudged by jury.";
            else
                await Facade.Rejudgings.RejudgeAsync(sub, fullTest: true);
            return RedirectToAction(nameof(Detail));
        }


        [HttpGet("{sid}/[action]/{jid}/{rid}")]
        public async Task<IActionResult> RunDetails(int pid, int sid, int jid, int rid)
        {
            var run = await Facade.Judgings.GetDetailAsync(pid, sid, jid, rid);
            if (run == null) return NotFound();
            ViewBag.CombinedRunCompare = Problem.CombinedRunCompare;
            return Window(run);
        }


        [HttpGet("{sid}/[action]/{jid}/{rid}/{type}")]
        public async Task<IActionResult> RunDetails(int pid, int sid, int jid, int rid, string type)
        {
            if (type == "meta")
            {
                var run = await Facade.Judgings.GetDetailAsync(pid, sid, jid, rid);
                if (run == null) return NotFound();
                return File(
                    fileContents: Convert.FromBase64String(run.MetaData),
                    contentType: "text/plain",
                    fileDownloadName: $"j{jid}.r{rid}.{type}");
            }
            else
            {
                var fileInfo = await Facade.Judgings.GetRunFileAsync(jid, rid, type, sid, pid);
                if (!fileInfo.Exists) return NotFound();

                return File(
                    fileStream: fileInfo.CreateReadStream(),
                    contentType: "application/octet-stream",
                    fileDownloadName: $"j{jid}.r{rid}.{type}");
            }
        }


        [HttpGet("[action]")]
        [ValidateAjaxWindow]
        [AtLeastLevel(AuthorLevel.Writer)]
        public IActionResult Rejudge()
        {
            return AskPost(
                title: "Rejudge all",
                message: "Do you want to rejudge all polygon submissions? " +
                    "This may take time and cause server load.",
                area: "Polygon", controller: "Submissions", action: "Rejudge",
                routeValues: new { pid = Problem.Id },
                type: BootstrapColor.warning);
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        [AtLeastLevel(AuthorLevel.Writer)]
        public async Task<IActionResult> Rejudge(int pid)
        {
            await Facade.Rejudgings.BatchRejudgeAsync(
                (s, j) => s.ExpectedResult != null && s.ProblemId == pid && s.ContestId == 0);
            StatusMessage = "All submissions are being rejudged.";
            return RedirectToAction(nameof(List));
        }


        [HttpGet("{sid}/[action]")]
        [ValidateAjaxWindow]
        public async Task<IActionResult> ChangeExpected(int pid, int sid)
        {
            var sub = await Facade.Submissions.FindAsync(sid);
            if (sub == null || sub.ProblemId != pid) return NotFound();
            ViewBag.Languages = await Facade.Languages.ListAsync();

            return Window(new ChangeExpectedModel
            {
                Verdict = !sub.ExpectedResult.HasValue ? -1 : (int)sub.ExpectedResult.Value,
                Language = sub.Language,
            });
        }


        [HttpPost("{sid}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeExpected(int pid, int sid, ChangeExpectedModel model)
        {
            var it = await Facade.Submissions.FindAsync(sid);
            if (it == null || it.ProblemId != pid) return NotFound();

            var expected = model.Verdict == -1 ? default(Verdict?) : (Verdict)model.Verdict;
            var language = model.Language;

            if (null == await Facade.Languages.FindAsync(language)) return NotFound();

            await Facade.Submissions.UpdateAsync(
                it,
                s => new Submission
                {
                    ExpectedResult = expected,
                    Language = language
                });

            return RedirectToAction(nameof(Detail));
        }
    }
}
