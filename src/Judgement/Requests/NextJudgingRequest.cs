using MediatR;

namespace Xylab.Polygon.Judgement.Requests
{
    public class NextJudgingRequest : IRequest<NextJudging?>
    {
        public string HostName { get; }

        public NextJudgingRequest(string hostname)
        {
            HostName = hostname;
        }
    }
}
