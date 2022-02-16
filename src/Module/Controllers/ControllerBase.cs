using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Polygon.Entities;
using Polygon.Models;
using Polygon.Packaging;
using Polygon.Storages;
using System.Threading.Tasks;

namespace SatelliteSite.PolygonModule.Controllers
{
    /// <summary>
    /// A base class for Polygon Module controllers.
    /// </summary>
    public abstract class PolygonControllerBase : ViewControllerBase
    {
        private IPolygonFacade _facade;
        private IMediator _mediator;

        /// <summary>
        /// The facade
        /// </summary>
        protected IPolygonFacade Facade => _facade;

        /// <summary>
        /// The messaging center
        /// </summary>
        protected IMediator Mediator => _mediator;

        /// <summary>
        /// The context problem
        /// </summary>
        protected new Problem Problem { get; private set; }

        /// <summary>
        /// The context author level
        /// </summary>
        protected AuthorLevel AuthorLevel { get; private set; }

        /// <summary>
        /// Validate the problem is ok.
        /// </summary>
        /// <returns>If validation passed, <c>null</c>.</returns>
        private async Task<IActionResult> ValidateAsync()
        {
            var feature = HttpContext.Features.Get<IPolygonFeature>();
            if (feature == null)
            {
                if (!RouteData.Values.TryGetValue("probid", out var _probid) ||
                    !int.TryParse(_probid as string, out int probid) ||
                    !User.IsSignedIn())
                    return NotFound();

                var results = await Facade.Problems.FindAsync(probid, User);
                if (results.Item1 == null || results.Item2 == null) return NotFound();
                feature = new PolygonFeature(results.Item1, results.Item2.Value);
                HttpContext.Features.Set<IPolygonFeature>(feature);
            }

            ViewData["ProblemItself"] = Problem = feature.Problem;
            ViewData["AuthorLevel"] = AuthorLevel = feature.AuthorLevel;
            ViewData["BigTitle"] = "Polygon";
            ViewData["NavbarName"] = Polygon.ResourceDictionary.MenuNavbar;
            ViewData["BigUrl"] = Url.Action("Overview", "Editor");

            var atLeast = HttpContext.GetEndpoint().Metadata.GetMetadata<AtLeastLevelAttribute>();
            if (atLeast != null && atLeast.Level > AuthorLevel) return StatusCode(403);

            return null;
        }

        /// <inheritdoc />
        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            _facade = context.GetService<IPolygonFacade>();
            _mediator = context.GetService<IMediator>();
            context.Result = await ValidateAsync();
            await base.OnActionExecutionAsync(context, next);
        }

        /// <summary>
        /// Read the statement for problem.
        /// </summary>
        /// <returns>A <see cref="Task"/> for reading the statement.</returns>
        protected Task<Statement> StatementAsync()
        {
            return HttpContext.GetService<IStatementProvider>().ReadAsync(Problem);
        }
    }
}
