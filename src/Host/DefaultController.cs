using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Xylab.Polygon.Storages;

namespace SatelliteSite
{
    [Area("Host")]
    [SupportStatusCodePage]
    [Route("[action]")]
    public class DefaultController : ViewControllerBase
    {
        [Route("/")]
        public IActionResult Home()
        {
            return RedirectToAction("Index", "Root", new { area = "Dashboard" });
        }


        [Route("/sql-case1")]
        public async Task<IActionResult> SqlCase1([FromServices] IPolygonFacade facade)
        {
            var list = await facade.Submissions.ListWithJudgingAsync(s => s.Id == 1, true, 75);
            return Json(list);
        }
    }
}
