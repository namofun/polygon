using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Polygon.Entities;
using Polygon.Models;
using Polygon.Packaging;
using Polygon.Storages;
using System;
using System.Threading.Tasks;

namespace SatelliteSite.PolygonModule.Controllers
{
    /// <summary>
    /// A base class for Polygon Module controllers.
    /// </summary>
    public abstract class PolygonControllerBase : ViewControllerBase
    {
        private IPolygonFacade _facade;

        /// <summary>
        /// The facade
        /// </summary>
        protected IPolygonFacade Facade => _facade;

        /// <summary>
        /// The context problem
        /// </summary>
        protected new Problem Problem { get; set; }

        /// <summary>
        /// Creates an <see cref="NotFoundResult"/> that produces a <see cref="Microsoft.AspNetCore.Http.StatusCodes.Status404NotFound"/> response.
        /// </summary>
        /// <returns>The created <see cref="NotFoundResult"/> for the response.</returns>
        protected new IActionResult NotFound() => StatusCodePage(404);

        /// <summary>
        /// Validate the problem is ok.
        /// </summary>
        /// <returns>If validation passed, <c>null</c>.</returns>
        private async Task<IActionResult> ValidateAsync()
        {
            if (HttpContext.Features.Get<IPolygonFeature>() is { Problem: var problem })
            {
                ViewData["ProblemItself"] = Problem = problem;
                return null;
            }

            if (!RouteData.Values.TryGetValue("pid", out var _pid) ||
                !int.TryParse(_pid as string, out int pid) ||
                !User.IsSignedIn())
                return base.NotFound();

            if (!User.IsInRole("Administrator"))
                Problem = await Facade.Problems.FindByPermissionAsync(pid, int.Parse(User.GetUserId() ?? "0"));
            else
                Problem = await Facade.Problems.FindAsync(pid);

            if (Problem == null) return base.NotFound();
            ViewData["ProblemItself"] = Problem;
            HttpContext.Features.Set<IPolygonFeature>(new PolygonFeature(Problem));
            return null;
        }

        /// <summary>
        /// Read the file of problem repository.
        /// </summary>
        /// <param name="fileName">The file name.</param>
        /// <returns>The task for content.</returns>
        protected async Task<string> ReadFileAsync(string fileName)
        {
            var fileInfo = await Facade.Problems.GetFileAsync(Problem.Id, fileName);
            return await fileInfo.ReadAsync();
        }

        /// <inheritdoc />
        public override async Task OnActionExecutionAsync(
            ActionExecutingContext context,
            ActionExecutionDelegate next)
        {
            _facade = context.GetService<IPolygonFacade>();
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
