using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Polygon;
using Polygon.Entities;
using Polygon.Storages;
using SatelliteSite.IdentityModule.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.PolygonModule.Dashboards
{
    [Area("Dashboard")]
    [Route("[area]/[controller]")]
    [Authorize(Roles = "Administrator,Problem")]
    [AuditPoint(Entities.AuditlogType.Problem)]
    public class ProblemsController : ViewControllerBase
    {
        private IPolygonFacade Facade { get; }
        private IProblemStore Store => Facade.Problems;
        private IUserManager UserManager { get; }
        private ILogger<ProblemsController> Logger { get; }
        private IOptions<PolygonOptions> Options { get; }

        public ProblemsController(
            IPolygonFacade facade,
            IUserManager userManager,
            ILogger<ProblemsController> logger,
            IOptions<PolygonOptions> options)
        {
            Facade = facade;
            UserManager = userManager;
            Logger = logger;
            Options = options;
        }

        private async Task<bool> CreateProblemRole(int problemId)
        {
            if (User.IsInRole("Administrator")) return true;

            var role = UserManager.CreateEmptyRole("AuthorOfProblem" + problemId);
            ((IRoleWithProblem)role).ProblemId = problemId;
            var i1 = await UserManager.CreateAsync(role);

            if (!i1.Succeeded)
            {
                StatusMessage = "Error creating roles. Please contact Administrator.";
                Logger.LogError(i1);
                return false;
            }

            var u = await UserManager.GetUserAsync(User);
            var i2 = await UserManager.AddToRoleAsync(u, "AuthorOfProblem" + problemId);

            if (!i2.Succeeded)
            {
                StatusMessage = "Error assigning roles. Please contact Administrator.";
                Logger.LogError(i2);
                return false;
            }

            return true;
        }


        [HttpGet]
        public async Task<IActionResult> List(int page = 1)
        {
            if (page < 1) return NotFound();
            var uid = User.IsInRole("Administrator")
                ? default(int?)
                : int.Parse(User.GetUserId());

            var model = await Store.ListAsync(page, 50, uid);
            return View(model);
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Create()
        {
            var p = await Store.CreateAsync(new Problem
            {
                AllowJudge = false,
                AllowSubmit = false,
                CompareScript = "compare",
                RunScript = "run",
                Title = "UNTITLED",
                MemoryLimit = 524288,
                OutputLimit = 4096,
                Source = string.Empty,
                TimeLimit = 10000,
                TagName = string.Empty,
            });

            await HttpContext.AuditAsync("created", $"{p.Id}");

            if (!await CreateProblemRole(p.Id))
                return RedirectToAction(nameof(List));

            return RedirectToAction(
                actionName: "Overview",
                controllerName: "Editor",
                routeValues: new { area = "Polygon", pid = p.Id });
        }


        [HttpGet("/[area]/submissions/[action]/{jid}")]
        public async Task<IActionResult> ByJudgingId(int jid)
        {
            var item = await Facade.Submissions.FindByJudgingAsync(jid);
            if (item == null) return NotFound();

            if (item.ContestId == 0)
                return RedirectToAction(
                    actionName: "Detail",
                    controllerName: "Submissions",
                    new { area = "Polygon", sid = item.Id, jid, pid = item.ProblemId });
            else
                return RedirectToAction(
                    actionName: "Detail",
                    controllerName: "Submissions",
                    new { area = "Contest", cid = item.ContestId, sid = item.Id, jid });
        }


        [HttpGet("[action]")]
        public IActionResult Import()
        {
            return Window(Options.Value.ImportProviders);
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        [RequestSizeLimit(1 << 30)]
        [RequestFormLimits2(1 << 30)]
        public async Task<IActionResult> Import(IFormFile file, string type)
        {
            try
            {
                var importer = Options.Value.CreateImportProviders(HttpContext.RequestServices, type);
                if (importer == null) return BadRequest();

                List<Problem> probs;
                using (var stream = file.OpenReadStream())
                    probs = await importer.ImportAsync(stream, file.FileName, User.GetUserName());

                if (probs.Count > 1)
                    importer.LogBuffer.AppendLine("Uploading multiple problems, showing first.");

                if (probs.Count == 0)
                    throw new InvalidOperationException("No problems are uploaded.");

                StatusMessage = importer.LogBuffer.ToString();

                foreach (var prob in probs)
                    await CreateProblemRole(prob.Id);

                return RedirectToAction(
                    actionName: "Overview",
                    controllerName: "Editor",
                    routeValues: new { area = "Polygon", pid = probs[0].Id });
            }
            catch (Exception ex)
            {
                Logger.LogError(ex, "An error occurred when importing problem packages.");

                return Message("Problem Import",
                    "Import failed. Please contact Administrator immediately. " + ex,
                    BootstrapColor.danger);
            }
        }


        [HttpGet("/[area]/submissions")]
        public async Task<IActionResult> Status(int page = 1)
        {
            if (page <= 0) return BadRequest();

            var model = await Facade.Submissions.ListWithJudgingAsync(pagination: (page, 50));

            var maxSub = model.Select(s => s.SubmissionId).Append(-1).Max();
            var minSub = model.Select(s => s.SubmissionId).Append(-1).Min();
            ViewBag.Authors = await Facade.Submissions.GetAuthorNamesAsync(
                sids: s => s.Id >= minSub && s.Id <= maxSub);

            ViewBag.Problems = await Store.ListNameAsync(
                (Submission s) => s.Id >= minSub && s.Id <= maxSub);

            return View(model);
        }


        [HttpGet("/[area]/submissions/[action]")]
        [Authorize(Roles = "Administrator")]
        public IActionResult RefreshCache()
        {
            return AskPost(
                title: "Rebuild submission statistics",
                message: "Are you sure to rebuild this cache? This will cause heavy load to the database.",
                area: "Dashboard", controller: "Problems", action: "RefreshCache",
                type: BootstrapColor.warning);
        }


        [HttpPost("/[area]/submissions/[action]")]
        [Authorize(Roles = "Administrator")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RefreshCache(bool post = true)
        {
            await Store.RebuildStatisticsAsync();
            StatusMessage = "Submission statistics has been rebuilt.";
            return RedirectToAction(nameof(Status));
        }
    }
}
