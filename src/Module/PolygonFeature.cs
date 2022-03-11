using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Routing;
using System;
using Xylab.Polygon.Entities;

namespace Microsoft.AspNetCore.Mvc
{
    public interface IPolygonFeature
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

    internal class AccessorFeature : IPolygonFeature, IViewContextAware
    {
        private IPolygonFeature _innerFeature;

        public Problem Problem => _innerFeature?.Problem ?? throw new InvalidOperationException();

        public AuthorLevel AuthorLevel => _innerFeature?.AuthorLevel ?? throw new InvalidOperationException();

        public void Contextualize(ViewContext viewContext)
        {
            _innerFeature = viewContext.HttpContext.Features.Get<IPolygonFeature>();
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
