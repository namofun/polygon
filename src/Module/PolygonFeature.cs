using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Polygon.Entities;

namespace Microsoft.AspNetCore.Mvc
{
    internal interface IPolygonFeature
    {
        Problem Problem { get; }
    }

    internal class PolygonFeature : IPolygonFeature
    {
        public Problem Problem { get; }

        public PolygonFeature(Problem problem)
        {
            Problem = problem;
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
