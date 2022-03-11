using MediatR;

namespace Xylab.Polygon.Judgement.Requests
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
