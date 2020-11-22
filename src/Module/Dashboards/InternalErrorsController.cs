using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polygon.Entities;
using Polygon.Storages;
using System.Threading.Tasks;

namespace SatelliteSite.PolygonModule.Dashboards
{
    [Area("Dashboard")]
    [Authorize(Roles = "Administrator")]
    [Route("[area]/[controller]")]
    [AuditPoint(Entities.AuditlogType.InternalError)]
    public class InternalErrorsController : ViewControllerBase
    {
        private IInternalErrorStore Store { get; }
        public InternalErrorsController(IInternalErrorStore store) => Store = store;


        [HttpGet]
        public async Task<IActionResult> List()
        {
            return View(await Store.ListAsync());
        }

        
        [HttpGet("{eid}/{todo}")]
        public async Task<IActionResult> Mark(int eid, string todo,
            [FromServices] IProblemStore problems,
            [FromServices] IJudgehostStore judgehosts,
            [FromServices] ILanguageStore languages)
        {
            var ie = await Store.FindAsync(eid);
            if (ie is null) return NotFound();

            if (ie.Status == InternalErrorStatus.Open)
            {
                var nstat = todo == "resolve"
                          ? InternalErrorStatus.Resolved
                          : todo == "ignore"
                          ? InternalErrorStatus.Ignored
                          : InternalErrorStatus.Open;
                if (nstat == InternalErrorStatus.Open)
                    return NotFound();

                var toDisable = await Store.ResolveAsync(ie, nstat);

                if (toDisable != null)
                {
                    var kind = toDisable.Kind;
                    if (kind == "language")
                        await languages.ToggleJudgeAsync(toDisable.Language, true);
                    else if (kind == "judgehost")
                        await judgehosts.ToggleAsync(toDisable.HostName, true);
                    else if (kind == "problem")
                        await problems.ToggleJudgeAsync(toDisable.ProblemId.Value, true);
                }

                await HttpContext.AuditAsync($"mark as {todo}d", $"{eid}");
            }

            return RedirectToAction(nameof(Detail), new { eid });
        }


        [HttpGet("{eid}")]
        public async Task<IActionResult> Detail(int eid)
        {
            var ie = await Store.FindAsync(eid);
            if (ie is null) return NotFound();
            return View(ie);
        }
    }
}
