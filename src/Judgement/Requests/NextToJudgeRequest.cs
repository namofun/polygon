using MediatR;

namespace Polygon.Judgement
{
    public class NextToJudgeRequest : IRequest<TestcaseToJudge?>
    {
        public int JudgingId { get; }

        public NextToJudgeRequest(int id)
        {
            JudgingId = id;
        }
    }
}
