using Microsoft.AspNetCore.Mvc;
using Xylab.Polygon.Models;
using System.Threading.Tasks;

namespace SatelliteSite.PolygonModule.Components.SubmissionDetail
{
    public class SubmissionDetailViewComponent : ViewComponent
    {
        public Task<IViewComponentResult> InvokeAsync(ISubmissionDetail model)
        {
            return Task.FromResult<IViewComponentResult>(View("Default", model));
        }
    }
}
