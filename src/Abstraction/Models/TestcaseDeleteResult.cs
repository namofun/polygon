namespace Xylab.Polygon.Models
{
    public class TestcaseDeleteResult
    {
        public int? DeletedJudgingRuns { get; private init; }

        public bool Succeeded { get; private init; }

        public string? ErrorMessage { get; private init; }

        public static TestcaseDeleteResult Succeed(int count)
        {
            return new() { DeletedJudgingRuns = count, Succeeded = true };
        }

        public static TestcaseDeleteResult Fail(string reason)
        {
            return new() { ErrorMessage = reason, Succeeded = false };
        }
    }
}
