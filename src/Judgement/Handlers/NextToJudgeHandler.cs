using MediatR;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xylab.Polygon.Entities;

namespace Xylab.Polygon.Judgement
{
    public partial class DOMjudgeLikeHandlers : IRequestHandler<NextToJudgeRequest, TestcaseToJudge?>
    {
        public async Task<TestcaseToJudge?> Handle(NextToJudgeRequest request, CancellationToken cancellationToken)
        {
            int id = request.JudgingId;

            var stats = await Facade.Judgings.ListAsync(
                predicate: j => j.Id == id && j.Status == Verdict.Running,
                selector: j => new { JudgingId = j.Id, j.s.ProblemId, j.FullTest, j.s.p.AllowJudge },
                topCount: 1);

            var stat = stats.SingleOrDefault();
            if (stat is null || !stat.AllowJudge) return null;

            var result = await Facade.Judgings.GetDetailsAsync(
                problemId: stat.ProblemId, judgingId: id,
                selector: (t, d) => new { Status = (Verdict?)d!.Status, t });

            if (!stat.FullTest && result.Any(s => s.Status.HasValue && s.Status != Verdict.Accepted))
                return null;

            var item = result.FirstOrDefault(a => !a.Status.HasValue);
            if (item == null) return null;

            return new TestcaseToJudge(item.t);
        }
    }
}
