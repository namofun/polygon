using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Polygon.Entities;

namespace Microsoft.AspNetCore.Mvc
{
    internal interface IPolygonFeature
    {
        Problem Problem { get; }

        AuthorLevel AuthorLevel { get; }
    }

    internal class PolygonFeature : IPolygonFeature
    {
        public Problem Problem { get; }

        public AuthorLevel AuthorLevel { get; }

        public PolygonFeature(Problem problem, AuthorLevel level)
        {
            Problem = problem;
            AuthorLevel = level;
        }
    }

    internal class RequirePolygonFeatureConstraint : IRouteConstraint
    {
        public bool Match(HttpContext httpContext, IRouter route, string routeKey, RouteValueDictionary values, RouteDirection routeDirection)
        {
            return httpContext.Features.Get<IPolygonFeature>() != null;
        }
    }
}
