using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polygon.Storages;
using System;
using System.Threading.Tasks;

namespace SatelliteSite.PolygonModule.Apis
{
    /// <summary>
    /// The controller for executable RESTful API.
    /// </summary>
    [Area("Api")]
    [Authorize(AuthenticationSchemes = "Basic")]
    [Route("[area]/[controller]")]
    [Produces("application/json")]
    public class ExecutablesController : ApiControllerBase
    {
        /// <summary>
        /// Get the executable with the given ID
        /// </summary>
        /// <param name="target">The ID of the entity to get</param>
        /// <param name="store"></param>
        /// <response code="200">Base64-encoded executable contents</response>
        [HttpGet("{target}")]
        [Authorize(Roles = "Judgehost,Administrator")]
        public async Task<ActionResult<string>> OnGet(
            [FromRoute] string target,
            [FromServices] IExecutableStore store)
        {
            var exec = await store.FindAsync(target);
            if (exec is null) return NotFound();
            var base64encoded = Convert.ToBase64String(exec.ZipFile);
            return base64encoded;
        }
    }
}
