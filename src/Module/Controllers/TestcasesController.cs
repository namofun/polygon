using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Polygon.Entities;
using Polygon.Events;
using Polygon.Storages;
using SatelliteSite.PolygonModule.Models;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SatelliteSite.PolygonModule.Controllers
{
    [Area("Polygon")]
    [Route("[area]/{probid}/[controller]")]
    [AuditPoint(AuditlogType.Testcase)]
    public class TestcasesController : PolygonControllerBase
    {
        private ITestcaseStore Store => Facade.Testcases;


        [HttpGet]
        public async Task<IActionResult> Testcases()
        {
            ViewBag.Testcases = await Store.ListAsync(Problem.Id);
            return View(Problem);
        }


        [HttpGet("[action]")]
        [ValidateAjaxWindow]
        [AtLeastLevel(AuthorLevel.Writer)]
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
        [AtLeastLevel(AuthorLevel.Writer)]
        public async Task<IActionResult> Score(TestcaseScoreModel model)
        {
            int cnt = await Store.BatchScoreAsync(Problem.Id, model.Lower, model.Upper, model.Score);
            await Mediator.Publish(new ProblemModifiedEvent(Problem));
            await HttpContext.AuditAsync("scored", $"{model.Lower} ~ {model.Upper}");
            StatusMessage = $"Score set, {cnt} testcases affected.";
            return RedirectToAction(nameof(Testcases));
        }


        [HttpGet("{testid}/[action]")]
        [AtLeastLevel(AuthorLevel.Writer)]
        public async Task<IActionResult> Edit(int testid)
        {
            var tc = await Store.FindAsync(testid, Problem.Id);
            if (tc == null) return NotFound();

            ViewData["Title"] = $"Edit testcase t{testid}";
            return Window(new TestcaseUploadModel
            {
                ProblemId = Problem.Id,
                Description = tc.Description,
                IsSecret = tc.IsSecret,
                Point = tc.Point,
                CustomInput = tc.CustomInput,
                CustomOutput = tc.CustomOutput,
            });
        }


        [HttpPost("{testid}/[action]")]
        [ValidateAntiForgeryToken]
        [AtLeastLevel(AuthorLevel.Writer)]
        [RequestSizeLimit(1 << 30)]
        [RequestFormLimits2(1 << 30)]
        public async Task<IActionResult> Edit(int testid, TestcaseUploadModel model)
        {
            try
            {
                var last = await Store.FindAsync(testid, Problem.Id);
                if (last == null) return NotFound();

                (Func<Stream>, long)? inputf = null, outputf = null;
                if (model.InputContent != null)
                    inputf = (() => model.InputContent.OpenReadStream(), model.InputContent.Length);
                if (model.OutputContent != null)
                    outputf = (() => model.OutputContent.OpenReadStream(), model.OutputContent.Length);

                last.Description = model.Description ?? last.Description;
                last.IsSecret = model.IsSecret;
                last.Point = model.Point;
                last.CustomInput = string.IsNullOrWhiteSpace(model.CustomInput) ? null : model.CustomInput;
                last.CustomOutput = string.IsNullOrWhiteSpace(model.CustomOutput) ? null : model.CustomOutput;
                await Store.UpdateAsync(last, inputf, outputf);

                await Mediator.Publish(new ProblemModifiedEvent(Problem));
                await HttpContext.AuditAsync("modified", $"p{last.ProblemId}t{last.Id}");
                StatusMessage = $"Testcase t{testid} updated successfully.";
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
        [AtLeastLevel(AuthorLevel.Writer)]
        public IActionResult Create()
        {
            ViewData["Title"] = "Add new testcase";
            return Window("Edit", new TestcaseUploadModel
            {
                ProblemId = Problem.Id,
                IsSecret = true,
            });
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        [AtLeastLevel(AuthorLevel.Writer)]
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
                        CustomInput = model.CustomInput,
                        CustomOutput = model.CustomOutput,
                    });

                await Mediator.Publish(new ProblemModifiedEvent(Problem));
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


        [HttpGet("{testid}/[action]")]
        [AtLeastLevel(AuthorLevel.Writer)]
        public IActionResult Delete(int testid)
        {
            return AskPost(
                title: "Delete testcase t" + testid,
                message: "You're about to delete testcase t" + testid + ". Are you sure? " +
                    "This operation is irreversible, and will make heavy load and data loss.",
                area: "Polygon", controller: "Testcases", action: "Delete",
                routeValues: new { probid = Problem.Id, testid },
                type: BootstrapColor.danger);
        }


        [HttpPost("{testid}/[action]")]
        [ValidateAntiForgeryToken]
        [AtLeastLevel(AuthorLevel.Writer)]
        public async Task<IActionResult> Delete(int testid, bool _ = true)
        {
            var tc = await Store.FindAsync(testid, Problem.Id);
            if (tc == null) return NotFound();

            int dts = await Store.CascadeDeleteAsync(tc);
            await Mediator.Publish(new ProblemModifiedEvent(Problem));
            await HttpContext.AuditAsync("deleted", $"p{Problem.Id}t{testid}");

            StatusMessage = dts < 0
                ? "Error occurred during the deletion."
                : $"Testcase {testid} with {dts} runs deleted.";
            return RedirectToAction(nameof(Testcases));
        }


        [HttpGet("{testid}/[action]/{direction}")]
        [AtLeastLevel(AuthorLevel.Writer)]
        public async Task<IActionResult> Move(int testid, string direction)
        {
            bool up = false;
            if (direction == "up") up = true;
            else if (direction != "down") return NotFound();

            await Store.ChangeRankAsync(Problem.Id, testid, up);
            await Mediator.Publish(new ProblemModifiedEvent(Problem));
            await HttpContext.AuditAsync("moved", $"p{Problem.Id}t{testid}");
            return RedirectToAction(nameof(Testcases));
        }


        [HttpGet("{testid}/[action]/{filetype}")]
        public async Task<IActionResult> Fetch(int testid, string filetype)
        {
            if (filetype == "input") filetype = "in";
            else if (filetype == "output") filetype = "out";
            else return NotFound();

            var tc = await Store.FindAsync(testid, Problem.Id);
            var fileInfo = await Store.GetFileAsync(tc, filetype);
            if (!fileInfo.Exists) return NotFound();

            return File(
                fileStream: fileInfo.CreateReadStream(),
                contentType: "application/octet-stream",
                fileDownloadName: $"p{Problem.Id}.t{testid}.{filetype}");
        }
    }
}
