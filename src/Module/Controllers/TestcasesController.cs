using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Polygon.Entities;
using Polygon.Storages;
using SatelliteSite.PolygonModule.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SatelliteSite.PolygonModule.Controllers
{
    [Area("Polygon")]
    [Route("[area]/{pid}/[controller]")]
    [AuditPoint(Entities.AuditlogType.Testcase)]
    public class TestcasesController : PolygonControllerBase
    {
        private ITestcaseStore Store => Facade.Testcases;


        [HttpGet]
        public async Task<IActionResult> Testcases(int pid)
        {
            ViewBag.Testcases = await Store.ListAsync(pid);
            return View(Problem);
        }


        [HttpGet("[action]")]
        [ValidateAjaxWindow]
        public async Task<IActionResult> Score()
        {
            var upper = await Store.CountAsync(Problem.Id);

            return Window(new TestcaseScoreModel
            {
                Upper = upper,
                Lower = 1,
                Score = upper == 0 ? 100 : (100 / upper),
                ProblemId = Problem.Id,
            });
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Score(TestcaseScoreModel model)
        {
            int cnt = await Store.BatchScoreAsync(Problem.Id, model.Lower, model.Upper, model.Score);
            await HttpContext.AuditAsync("scored", $"{model.Lower} ~ {model.Upper}");
            StatusMessage = $"Score set, {cnt} testcases affected.";
            return RedirectToAction(nameof(Testcases));
        }


        [HttpGet("{tid}/[action]")]
        public async Task<IActionResult> Edit(int tid)
        {
            var tc = await Store.FindAsync(tid, Problem.Id);
            if (tc == null) return NotFound();

            ViewData["Title"] = $"Edit testcase t{tid}";
            return Window(new TestcaseUploadModel
            {
                ProblemId = Problem.Id,
                Description = tc.Description,
                IsSecret = tc.IsSecret,
                Point = tc.Point,
            });
        }


        [HttpPost("{tid}/[action]")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1 << 30)]
        [RequestFormLimits2(1 << 30)]
        public async Task<IActionResult> Edit(int tid, TestcaseUploadModel model)
        {
            try
            {
                var last = await Store.FindAsync(tid, Problem.Id);
                if (last == null) return NotFound();

                (Func<Stream>, long)? inputf = null, outputf = null;
                if (model.InputContent != null)
                    inputf = (() => model.InputContent.OpenReadStream(), model.InputContent.Length);
                if (model.OutputContent != null)
                    outputf = (() => model.OutputContent.OpenReadStream(), model.OutputContent.Length);

                last.Description = model.Description ?? last.Description;
                last.IsSecret = model.IsSecret;
                last.Point = model.Point;
                await Store.UpdateAsync(last, inputf, outputf);

                await HttpContext.AuditAsync("modified", $"p{last.ProblemId}t{last.Id}");
                StatusMessage = $"Testcase t{tid} updated successfully.";
                return RedirectToAction(nameof(Testcases));
            }
            catch (Exception ex)
            {
                return Message(
                    "Testcase Upload",
                    "Upload failed. Please contact Administrator. " + ex,
                    BootstrapColor.danger);
            }
        }


        [HttpGet("[action]")]
        public IActionResult Create()
        {
            ViewData["Title"] = "Add new testcase";
            return Window("Edit", new TestcaseUploadModel
            {
                ProblemId = Problem.Id,
            });
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1 << 30)]
        [RequestFormLimits2(1 << 30)]
        public async Task<IActionResult> Create(TestcaseUploadModel model)
        {
            if (model.InputContent == null)
                return Message("Create testcase", "No input file specified.", BootstrapColor.danger);
            if (model.OutputContent == null)
                return Message("Create testcase", "No output file specified.", BootstrapColor.danger);

            try
            {
                var e = await Store.CreateAsync(
                    inputFactory: () => model.InputContent.OpenReadStream(),
                    inputLength: model.InputContent.Length,
                    outputFactory: () => model.OutputContent.OpenReadStream(),
                    outputLength: model.OutputContent.Length,
                    entity: new Testcase
                    {
                        Description = model.Description ?? "1",
                        IsSecret = model.IsSecret,
                        Point = model.Point,
                        ProblemId = Problem.Id,
                    });

                await HttpContext.AuditAsync("created", $"p{Problem.Id}t{e.Id}");
                StatusMessage = $"Testcase t{e.Id} created successfully.";
                return RedirectToAction(nameof(Testcases));
            }
            catch (Exception ex)
            {
                return Message(
                    "Testcase Upload",
                    "Upload failed. Please contact Administrator. " + ex,
                    BootstrapColor.danger);
            }
        }


        [HttpGet("{tid}/[action]")]
        public IActionResult Delete(int tid)
        {
            return AskPost(
                title: "Delete testcase t" + tid,
                message: "You're about to delete testcase t" + tid + ". Are you sure? " +
                    "This operation is irreversible, and will make heavy load and data loss.",
                area: "Polygon", controller: "Testcases", action: "Delete",
                routeValues: new { pid = Problem.Id, tid },
                type: BootstrapColor.danger);
        }


        [HttpPost("{tid}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int pid, int tid)
        {
            var tc = await Store.FindAsync(tid, pid);
            if (tc == null) return NotFound();

            int dts = await Store.CascadeDeleteAsync(tc);
            await HttpContext.AuditAsync("deleted", $"p{pid}t{tid}");
            StatusMessage = dts < 0
                ? "Error occurred during the deletion."
                : $"Testcase {tid} with {dts} runs deleted.";
            return RedirectToAction(nameof(Testcases));
        }


        [HttpGet("{tid}/[action]/{direction}")]
        public async Task<IActionResult> Move(int tid, string direction)
        {
            bool up = false;
            if (direction == "up") up = true;
            else if (direction != "down") return NotFound();

            await Store.ChangeRankAsync(Problem.Id, tid, up);
            await HttpContext.AuditAsync("moved", $"p{Problem.Id}t{tid}");
            return RedirectToAction(nameof(Testcases));
        }


        [HttpGet("{tid}/[action]/{filetype}")]
        public async Task<IActionResult> Fetch(int tid, string filetype)
        {
            if (filetype == "input") filetype = "in";
            else if (filetype == "output") filetype = "out";
            else return NotFound();

            var tc = await Store.FindAsync(tid, Problem.Id);
            var fileInfo = await Store.GetFileAsync(tc, filetype);
            if (!fileInfo.Exists) return NotFound();

            return File(
                fileStream: fileInfo.CreateReadStream(),
                contentType: "application/octet-stream",
                fileDownloadName: $"p{Problem.Id}.t{tid}.{filetype}");
        }
    }
}
