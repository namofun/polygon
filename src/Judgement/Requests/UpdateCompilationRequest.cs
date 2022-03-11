using MediatR;
using Xylab.Polygon.Entities;

namespace Xylab.Polygon.Judgement.Requests
{
    public class UpdateCompilationRequest : IRequest<Judging?>
    {
        public int Success { get; }

        public string CompilerOutput { get; }

        public string Judgehost { get; }

        public int JudgingId { get; }

        public UpdateCompilationRequest(int succ, string outp, int judgingid, string host)
        {
            Success = succ;
            CompilerOutput = outp;
            Judgehost = host;
            JudgingId = judgingid;
        }
    }
}
