using System;
using System.Linq.Expressions;

namespace SatelliteSite
{
    public class TestQueryCache : QueryCache<TestContext>
    {
        protected override Expression<Func<DateTimeOffset, DateTimeOffset, double>> CalculateDuration { get; }
            = (start, end) => (end - start).TotalSeconds;
    }
}
