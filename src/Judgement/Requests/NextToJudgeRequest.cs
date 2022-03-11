using MediatR;

namespace Xylab.Polygon.Judgement
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
