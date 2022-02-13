namespace SatelliteSite
{
    public class TestQueryCache : QueryCache<TestContext>
    {
        public TestQueryCache()
            : base(
                  (start, end) => (end - start).TotalSeconds)
        {
        }
    }
}
