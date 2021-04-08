using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polygon.Entities;
using Polygon.Judgement;
using Polygon.Storages;
using SatelliteSite.PolygonModule.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.PolygonModule.Apis
{
    /// <summary>
    /// The functions to connect to DOMjudge Judgehosts.
    /// </summary>
    [Area("Api")]
    [Authorize(AuthenticationSchemes = "Basic")]
    [Authorize(Roles = "Judgehost")]
    [Route("[area]/[controller]")]
    [Produces("application/json")]
    public class JudgehostsController : ApiControllerBase
    {
        /// <summary>
        /// Get judgehosts
        /// </summary>
        /// <param name="hostname">Only show the judgehost with the given hostname</param>
        /// <param name="store"></param>
        /// <response code="200">The judgehosts</response>
        [HttpGet]
        public async Task<ActionResult<List<JudgehostInfo>>> OnGet(
            [FromQuery] string hostname,
            [FromServices] IJudgehostStore store)
        {
            List<Judgehost> hosts;

            if (hostname != null)
            {
                hosts = new List<Judgehost>();
                var host = await store.FindAsync(hostname);
                if (host != null) hosts.Add(host);
            }
            else
            {
                hosts = await store.ListAsync();
            }

            return hosts.Select(a => new JudgehostInfo(a)).ToList();
        }


        /// <summary>
        /// Add a new judgehost to the list of judgehosts
        /// </summary>
        /// <remarks>
        /// Also restarts (and returns) unfinished judgings.
        /// </remarks>
        /// <param name="hostname">The name of added judgehost</param>
        /// <param name="mediator"></param>
        /// <response code="200">The returned unfinished judgings</response>
        [HttpPost]
        [AuditPoint(AuditlogType.Judgehost)]
        public async Task<ActionResult<List<UnfinishedJudging>>> OnPost(
            [FromForm, Required] string hostname,
            [FromServices] IMediator mediator)
        {
            return await mediator.Send(
                new JudgehostRegisterRequest(
                    hostname,
                    User.GetUserName(),
                    HttpContext.Connection.RemoteIpAddress));
        }


        /// <summary>
        /// Update the configuration of the given judgehost
        /// </summary>
        /// <param name="hostname">The hostname of the judgehost to update</param>
        /// <param name="active">The new active state of the judgehost</param>
        /// <param name="store"></param>
        /// <response code="200">The modified judgehost</response>
        [HttpPut("{hostname}")]
        [Authorize(Roles = "Administrator")]
        [AuditPoint(AuditlogType.Judgehost)]
        public async Task<ActionResult<JudgehostInfo[]>> OnPut(
            [FromRoute] string hostname, bool active,
            [FromServices] IJudgehostStore store)
        {
            var item = await store.FindAsync(hostname);
            if (item == null) return NotFound();

            item.Active = active;
            await store.ToggleAsync(hostname, active);

            await HttpContext.AuditAsync(
                $"mark {(active ? "" : "in")}active", hostname,
                $"by api {HttpContext.Connection.RemoteIpAddress}");

            return new[] { new JudgehostInfo(item) };
        }


        /// <summary>
        /// Get the next judging for the given judgehost
        /// </summary>
        /// <param name="hostname">The hostname of the judgehost to get the next judging for</param>
        /// <param name="mediator"></param>
        /// <response code="200">The next judging to judge</response>
        [HttpPost("[action]/{hostname}")]
        public async Task<ActionResult<NextJudging>> NextJudging(
            [FromRoute] string hostname,
            [FromServices] IMediator mediator)
        {
            var result = await mediator.Send(new NextJudgingRequest(hostname));
            if (result == null) return new JsonResult("");
            await mediator.Publish(new Polygon.Events.JudgingPrepublishEvent(result));
            return result;
        }


        /// <summary>
        /// Update the given judging for the given judgehost
        /// </summary>
        /// <param name="hostname">The hostname of the judgehost that wants to update the judging</param>
        /// <param name="judgingId">The ID of the judging to update</param>
        /// <param name="mediator"></param>
        /// <param name="model">Model</param>
        /// <response code="200">When the judging has been updated</response>
        [HttpPut("[action]/{hostname}/{judgingId}")]
        [RequestSizeLimit(1 << 30)]
        [RequestFormLimits2(1 << 30)]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> UpdateJudging(
            [FromRoute] string hostname,
            [FromRoute] int judgingId,
            [FromForm] UpdateCompilation model,
            [FromServices] IMediator mediator)
        {
            var request = new UpdateCompilationRequest(model.compile_success, model.output_compile, judgingId, hostname);
            var result = await mediator.Send(request);
            if (result == null) return BadRequest();
            return Ok();
        }


        /// <summary>
        /// Add an array of JudgingRuns. When relevant, finalize the judging.
        /// </summary>
        /// <param name="hostname">The hostname of the judgehost that wants to add the judging run</param>
        /// <param name="judgingId">The ID of the judging to add a run to</param>
        /// <param name="batch">The judging run model (form-value encoded JSON array of JudgingRunModel)</param>
        /// <!--<param name="model">Model sample</param>-->
        /// <param name="mediator"></param>
        /// <response code="200">When the judging run has been added</response>
        [HttpPost("[action]/{hostname}/{judgingId}")]
        [RequestSizeLimit(1 << 30)]
        [RequestFormLimits2(1 << 30)]
        [Consumes("application/x-www-form-urlencoded")]
        public async Task<IActionResult> AddJudgingRun(
            [FromRoute] string hostname,
            [FromRoute] int judgingId,
            [FromForm, ModelBinder(typeof(JudgingRunBinder))] List<JudgingRunModel> batch,
            // [FromForm] JudgingRunModel model,
            [FromServices] IMediator mediator)
        {
            // if (batch is null && model?.TestcaseId != null)
            //     batch = new List<JudgingRunModel> { model };
            if (batch is null) return BadRequest();

            var request = new AddJudgingRunRequest(
                hostname: hostname,
                judgingid: judgingId,
                batchParser: (j, t) => batch.Select(b => b.ParseInfo(j, t)));
            var result = await mediator.Send(request);
            if (result) return Ok(); else return BadRequest();
        }


        /// <summary>
        ///  Internal error reporting (back from judgehost)
        /// </summary>
        /// <param name="model">Model</param>
        /// <param name="mediator"></param>
        /// <response code="200">The ID of the created internal error</response>
        [HttpPost("[action]")]
        [RequestSizeLimit(1 << 26)]
        [Consumes("application/x-www-form-urlencoded")]
        [AuditPoint(AuditlogType.InternalError)]
        public async Task<ActionResult<int>> InternalError(
            [FromForm] InternalErrorModel model,
            [FromServices] IMediator mediator)
        {
            if (!ModelState.IsValid) return BadRequest();
            var request = new InternalErrorOccurrence(model.description, model.judgehostlog, model.disabled, model.cid, model.judgingid);
            var (ie, disable) = await mediator.Send(request);
            await HttpContext.AuditAsync("added", $"{ie.Id}", $"for {disable.Kind}");
            return ie.Id;
        }
    }
}
