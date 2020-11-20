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

        public UpdateCompilationRequest(UpdateCompilation model, int jid, string host)
        {
            Success = model.compile_success;
            CompilerOutput = model.output_compile;
            Judgehost = host;
            JudgingId = jid;
        }
    }
}
