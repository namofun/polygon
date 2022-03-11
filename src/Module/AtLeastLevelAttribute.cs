using System;
using Xylab.Polygon.Entities;

namespace SatelliteSite.PolygonModule
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public sealed class AtLeastLevelAttribute : Attribute
    {
        public AuthorLevel Level { get; }

        public AtLeastLevelAttribute(AuthorLevel level)
        {
            Level = level;
        }
    }
}
