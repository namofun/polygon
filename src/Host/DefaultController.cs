using Microsoft.AspNetCore.Mvc;

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
    }
}
