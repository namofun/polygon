using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polygon.Judgement;
using Polygon.Storages;
using System.Threading.Tasks;

namespace SatelliteSite.PolygonModule.Apis
{
    /// <summary>
    /// The controller for fetching testcases.
    /// </summary>
    [Area("Api")]
    [Authorize(AuthenticationSchemes = "Basic")]
    [Authorize(Roles = "Judgehost")]
    [Route("[area]/[controller]")]
    [Produces("application/json")]
    public class TestcasesController : ApiControllerBase
    {
        /// <summary>
        /// Get the next to judge testcase for the given judging ID
        /// </summary>
        /// <param name="id">The ID of the entity to get</param>
        /// <param name="mediator"></param>
        /// <response code="200">Information about the next testcase to run</response>
        [HttpGet("[action]/{id}")]
        public async Task<ActionResult<TestcaseToJudge>> NextToJudge(
            [FromRoute] int id,
            [FromServices] IMediator mediator)
        {
            var result = await mediator.Send(new NextToJudgeRequest(id));
            if (result == null) return new JsonResult("");
            return result;
        }


        /// <summary>
        /// Get the input or output file for the given testcase
        /// </summary>
        /// <param name="id">The ID of the entity to get</param>
        /// <param name="type">Type of file to get</param>
        /// <param name="testcases"></param>
        /// <response code="200">Information about the file of the given testcase</response>
        [HttpGet("{id}/[action]/{type}")]
        [Produces("application/json", "application/octet-stream")]
        public async Task<ActionResult<string>> File(
            [FromRoute] int id,
            [FromRoute] string type,
            [FromServices] ITestcaseStore testcases)
        {
            if (type == "input") type = "in";
            else if (type == "output") type = "out";
            else return BadRequest();

            var tc = await testcases.FindAsync(id);
            if (tc is null) return NotFound();

            var fileInfo = await testcases.GetFileAsync(tc, type);
            if (!fileInfo.Exists) return NotFound();
            return this.Output(fileInfo);
        }
    }
}
