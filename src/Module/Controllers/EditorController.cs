using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Polygon.Entities;
using Polygon.Packaging;
using Polygon.Storages;
using SatelliteSite.PolygonModule.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.PolygonModule.Controllers
{
    [Area("Polygon")]
    [Route("[area]/{pid}/[action]")]
    [SupportStatusCodePage]
    public class EditorController : PolygonControllerBase
    {
        private IProblemStore Store => Facade.Problems;


        [HttpGet("/[area]/{pid}")]
        public async Task<IActionResult> Overview()
        {
            var (count, score) = await Facade.Testcases.CountAndScoreAsync(Problem.Id);
            ViewBag.TestcaseCount = count;
            ViewBag.TestcaseScore = score;
            ViewBag.Users = await Facade.Problems.ListPermittedUserAsync(Problem.Id);
            return View(Problem);
        }


        [HttpGet("{execid}")]
        public async Task<IActionResult> Executables(string execid)
        {
            if (execid != Problem.CompareScript && execid != Problem.RunScript)
                return NotFound();
            var bytes = await Facade.Executables.FindAsync(execid);
            if (bytes is null) return NotFound();

            ViewBag.Executable = bytes;
            var items = await Facade.Executables.FetchContentAsync(bytes);
            return View(items);
        }


        [HttpGet]
        [AtLeastLevel(AuthorLevel.Writer)]
        public IActionResult Edit()
        {
            return View(new ProblemEditModel
            {
                ProblemId = Problem.Id,
                CompareScript = Problem.CompareScript,
                RunScript = Problem.RunScript,
                RunAsCompare = Problem.CombinedRunCompare,
                CompareArgument = Problem.CompareArguments,
                Source = Problem.Source,
                MemoryLimit = Problem.MemoryLimit,
                TimeLimit = Problem.TimeLimit,
                OutputLimit = Problem.OutputLimit,
                Title = Problem.Title,
                Shared = Problem.Shared,
                Tags = Problem.TagName,
            });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [AtLeastLevel(AuthorLevel.Writer)]
        [AuditPoint(AuditlogType.Problem)]
        public async Task<IActionResult> Edit(ProblemEditModel model)
        {
            if (model.RunScript == "upload" && model.UploadedRun == null)
                ModelState.AddModelError("XYS::RunScript", "No run script was selected.");
            if (model.CompareScript == "upload" && model.UploadedCompare == null)
                ModelState.AddModelError("XYS::CmpScript", "No compare script was selected.");
            if (!new[] { "compare", Problem.CompareScript, "upload" }.Contains(model.CompareScript))
                ModelState.AddModelError("XYS::CmpScript", "Error compare script defined.");
            if (!new[] { "run", Problem.RunScript, "upload" }.Contains(model.RunScript))
                ModelState.AddModelError("XYS::RunScript", "Error run script defined.");

            if (!ModelState.IsValid)
            {
                StatusMessage = "Error validating problem.\n" +
                    string.Join('\n', ModelState.Values
                        .SelectMany(m => m.Errors)
                        .Select(e => e.ErrorMessage));
                return View(model);
            }

            if (model.RunScript == "upload")
            {
                var cont = await model.UploadedRun.ReadAsync();
                var execid = $"p{Problem.Id}run";

                var exec = await Facade.Executables.FindAsync(execid);
                bool newone = exec == null;
                exec ??= new Executable();
                exec.Id = execid;
                exec.Description = $"run pipe for p{Problem.Id}";
                exec.Md5sum = cont.Item2;
                exec.ZipFile = cont.Item1;
                exec.Type = "run";
                exec.ZipSize = cont.Item1.Length;

                if (newone) await Facade.Executables.CreateAsync(exec);
                else await Facade.Executables.UpdateAsync(exec);
                model.RunScript = execid;
            }

            if (model.CompareScript == "upload")
            {
                var cont = await model.UploadedCompare.ReadAsync();
                var execid = $"p{Problem.Id}cmp";

                var exec = await Facade.Executables.FindAsync(execid);
                bool newone = exec == null;
                exec ??= new Executable();
                exec.Id = execid;
                exec.Description = $"output validator for p{Problem.Id}";
                exec.Md5sum = cont.Item2;
                exec.ZipFile = cont.Item1;
                exec.Type = "compare";
                exec.ZipSize = cont.Item1.Length;

                if (newone) await Facade.Executables.CreateAsync(exec);
                else await Facade.Executables.UpdateAsync(exec);
                model.CompareScript = execid;
            }

            model.Source ??= string.Empty;
            model.Tags ??= string.Empty;

            await Store.UpdateAsync(Problem, _ => new Problem
            {
                RunScript = model.RunScript,
                CompareScript = model.CompareScript,
                CompareArguments = model.CompareArgument,
                MemoryLimit = model.MemoryLimit,
                OutputLimit = model.OutputLimit,
                TimeLimit = model.TimeLimit,
                Title = model.Title,
                Source = model.Source,
                TagName = model.Tags,
                CombinedRunCompare = model.RunAsCompare,
                Shared = model.Shared,
            });

            await HttpContext.AuditAsync("edit", $"{Problem.Id}");
            return RedirectToAction(nameof(Overview));
        }


        [HttpGet]
        [AtLeastLevel(AuthorLevel.Creator)]
        public IActionResult Delete()
        {
            return AskPost(
                title: $"Delete problem {Problem.Id} - \"{Problem.Title}\"",
                message: $"You're about to delete problem {Problem.Id} \"{Problem.Title}\". " +
                    "Warning, this will cascade to testcases and submissions. Are you sure?",
                area: "Polygon", controller: "Editor", action: "Delete",
                routeValues: new { pid = Problem.Id },
                type: BootstrapColor.danger);
        }


        [HttpGet]
        [AtLeastLevel(AuthorLevel.Writer)]
        [AuditPoint(AuditlogType.Problem)]
        public async Task<IActionResult> Export(
            [FromServices] IExportProvider export)
        {
            await HttpContext.AuditAsync("export", $"{Problem.Id}");
            var result = await export.ExportAsync(Problem);
            return File(result.OpenStream, result.MimeType, result.FileName, false);
        }


        [HttpGet("{uid}")]
        [AtLeastLevel(AuthorLevel.Creator)]
        public async Task<IActionResult> Unauthorize(
            [FromRoute] int uid,
            [FromServices] IUserManager userManager)
        {
            var user = await userManager.FindByIdAsync(uid);
            if (user == null || (!User.IsInRole("Administrator") && user.UserName == User.GetUserName())) return NotFound();

            return AskPost(
                title: "Unassign authority",
                message: $"Are you sure to unassign user {user.UserName} (u{uid})?",
                area: "Polygon", controller: "Editor", action: "Unauthorize");
        }


        [HttpPost("{uid}")]
        [ValidateAntiForgeryToken]
        [AtLeastLevel(AuthorLevel.Creator)]
        public async Task<IActionResult> Unauthorize(
            [FromServices] IUserManager userManager,
            [FromRoute] int uid)
        {
            var user = await userManager.FindByIdAsync(uid);
            if (user == null || (!User.IsInRole("Administrator") && user.UserName == User.GetUserName())) return NotFound();
            await Store.AuthorizeAsync(Problem.Id, user.Id, null);
            StatusMessage = "Role unassigned.";
            return RedirectToAction(nameof(Overview));
        }


        [HttpGet]
        [AtLeastLevel(AuthorLevel.Creator)]
        public IActionResult Authorize()
        {
            return Window();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [AtLeastLevel(AuthorLevel.Creator)]
        public async Task<IActionResult> Authorize(
            string username,
            AuthorLevel level,
            [FromServices] IUserManager userManager)
        {
            if (level < AuthorLevel.Reader || level > AuthorLevel.Creator)
            {
                return BadRequest();
            }

            var names = username.Split(',', StringSplitOptions.RemoveEmptyEntries);
            var users = new List<IUser>();

            foreach (var name in names)
            {
                var user = await userManager.FindByNameAsync(name);

                if (user == null)
                {
                    StatusMessage = "Error user not found.";
                    return RedirectToAction(nameof(Overview));
                }

                users.Add(user);
            }

            foreach (var user in users)
            {
                await Store.AuthorizeAsync(Problem.Id, user.Id, level);
            }

            StatusMessage = "Role assigned.";
            return RedirectToAction(nameof(Overview));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [AtLeastLevel(AuthorLevel.Writer)]
        public async Task<IActionResult> ToggleSubmit()
        {
            await Store.ToggleSubmitAsync(Problem.Id, !Problem.AllowSubmit);
            return RedirectToAction(nameof(Overview));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [AtLeastLevel(AuthorLevel.Writer)]
        public async Task<IActionResult> ToggleJudge()
        {
            await Store.ToggleJudgeAsync(Problem.Id, !Problem.AllowJudge);
            return RedirectToAction(nameof(Overview));
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [AtLeastLevel(AuthorLevel.Creator)]
        [AuditPoint(AuditlogType.Problem)]
        public async Task<IActionResult> Delete(int pid)
        {
            try
            {
                await Store.DeleteAsync(Problem);
                await HttpContext.AuditAsync("deleted", $"{Problem.Id}");
                StatusMessage = $"Problem {Problem.Id} deleted successfully.";
                return RedirectToAction("List", "Problems", new { area = "Dashboard" });
            }
            catch
            {
                StatusMessage = $"Error occurred when deleting Problem {Problem.Id}, foreign key constraints failed.";
                return RedirectToAction(nameof(Overview));
            }
        }
    }
}
