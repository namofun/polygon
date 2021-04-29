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
    [Route("[area]/{probid}/[controller]")]
    public class SubmissionsController : PolygonControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> List(bool all = false, int page = 1)
        {
            if (page < 0) return NotFound();

            var cond = Expr.Of<Submission>(s => s.ProblemId == Problem.Id)
                .CombineIf(!all, s => s.ExpectedResult != null);
            var result = await Facade.Submissions.ListWithJudgingAsync((page, 30), cond);

            if (result.Count > 0)
            {
                var sids = result.Select(s => s.SubmissionId);
                var names = await Facade.Submissions.GetAuthorNamesAsync(s => sids.Contains(s.Id));
                foreach (var item in result)
                    item.AuthorName = names.GetValueOrDefault(item.SubmissionId, "SYSTEM");

                var jids = result.Select(s => s.JudgingId);
                ViewBag.JudgingRuns = await Facade.Judgings.GetJudgingRunsAsync(jids);
            }
            else
            {
                ViewBag.JudgingRuns = new NullLookup<int, JudgingRun>();
            }

            ViewBag.All = all;
            ViewBag.Testcase = await Facade.Testcases.ListAsync(Problem.Id);
            return View(result);
        }


        [HttpGet("{submitid}")]
        public async Task<IActionResult> Detail(int submitid, int? judgingid)
        {
            var s = await Facade.Submissions.FindAsync(submitid, true);
            if (s == null || s.ProblemId != Problem.Id) return NotFound();
            var j = s.Judgings.SingleOrDefault(jj => judgingid.HasValue ? jj.Id == judgingid : jj.Active);
            if (j == null) return NotFound();
            var l = await Facade.Languages.FindAsync(s.Language);
            var det = await Facade.Judgings.GetDetailsAsync(Problem.Id, j.Id);
            var uname = await Facade.Submissions.GetAuthorNameAsync(submitid);

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

            return RedirectToAction(nameof(Detail), new { submitid = sub.Id });
        }


        [HttpGet("{submitid}/rejudge")]
        public async Task<IActionResult> RejudgeOne(int submitid)
        {
            var sub = await Facade.Submissions.FindAsync(submitid);
            if (sub == null || sub.ProblemId != Problem.Id)
                return NotFound();

            if (sub.ContestId != 0)
                StatusMessage = "Error contest submissions should be rejudged by jury.";
            else
                await Facade.Rejudgings.RejudgeAsync(sub, fullTest: true);
            return RedirectToAction(nameof(Detail));
        }


        [HttpGet("{submitid}/[action]/{judgingid}/{runid}")]
        public async Task<IActionResult> RunDetails(int submitid, int judgingid, int runid)
        {
            var run = await Facade.Judgings.GetDetailAsync(Problem.Id, submitid, judgingid, runid);
            if (run == null) return NotFound();
            ViewBag.CombinedRunCompare = Problem.CombinedRunCompare;
            return Window(run);
        }


        [HttpGet("{submitid}/[action]/{judgingid}/{runid}/{type}")]
        public async Task<IActionResult> RunDetails(int submitid, int judgingid, int runid, string type)
        {
            if (type == "meta")
            {
                var run = await Facade.Judgings.GetDetailAsync(Problem.Id, submitid, judgingid, runid);
                if (run == null) return NotFound();
                return File(
                    fileContents: Convert.FromBase64String(run.MetaData),
                    contentType: "text/plain",
                    fileDownloadName: $"j{judgingid}.r{runid}.{type}");
            }
            else
            {
                var fileInfo = await Facade.Judgings.GetRunFileAsync(judgingid, runid, type, submitid, Problem.Id);
                if (!fileInfo.Exists) return NotFound();

                return File(
                    fileStream: fileInfo.CreateReadStream(),
                    contentType: "application/octet-stream",
                    fileDownloadName: $"j{judgingid}.r{runid}.{type}");
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
                routeValues: new { probid = Problem.Id },
                type: BootstrapColor.warning);
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        [AtLeastLevel(AuthorLevel.Writer)]
        public async Task<IActionResult> Rejudge(bool _ = true)
        {
            await Facade.Rejudgings.BatchRejudgeAsync(
                (s, j) => s.ExpectedResult != null
                       && s.ProblemId == Problem.Id
                       && s.ContestId == 0);

            StatusMessage = "All submissions are being rejudged.";
            return RedirectToAction(nameof(List));
        }


        [HttpGet("{submitid}/[action]")]
        [ValidateAjaxWindow]
        public async Task<IActionResult> ChangeExpected(int submitid)
        {
            var sub = await Facade.Submissions.FindAsync(submitid);
            if (sub == null || sub.ProblemId != Problem.Id) return NotFound();
            ViewBag.Languages = await Facade.Languages.ListAsync();

            return Window(new ChangeExpectedModel
            {
                Verdict = !sub.ExpectedResult.HasValue ? -1 : (int)sub.ExpectedResult.Value,
                Language = sub.Language,
            });
        }


        [HttpPost("{submitid}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangeExpected(int submitid, ChangeExpectedModel model)
        {
            var it = await Facade.Submissions.FindAsync(submitid);
            if (it == null || it.ProblemId != Problem.Id) return NotFound();

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
