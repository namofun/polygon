using MediatR;
using Polygon.Entities;

namespace Polygon.Judgement
{
    public class UpdateCompilationRequest : IRequest<Judging?>
    {
        public int Success { get; }

        public string CompilerOutput { get; }

        public string Judgehost { get; }

        public int JudgingId { get; }

        public UpdateCompilationRequest(int succ, string outp, int jid, string host)
        {
            Success = succ;
            CompilerOutput = outp;
            Judgehost = host;
            JudgingId = jid;
        }
    }
}
