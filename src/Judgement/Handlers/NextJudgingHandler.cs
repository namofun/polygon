using MediatR;
using System.Threading;
using System.Threading.Tasks;
using Xylab.Polygon.Events;

namespace Xylab.Polygon.Judgement.Requests
{
    public partial class DOMjudgeLikeHandlers : IRequestHandler<NextJudgingRequest, NextJudging?>
    {
        public async Task<NextJudging?> Handle(NextJudgingRequest request, CancellationToken cancellationToken)
        {
            var host = await Facade.Judgehosts.FindAsync(request.HostName);
            if (host is null) return null;
            await Facade.Judgehosts.NotifyPollAsync(host);
            if (!host.Active) return null;
            // Above: unknown or inactive judgehost requested

            JudgingBeginEvent? r = await Facade.Judgings.DequeueAsync(host.ServerName);
            if (r is null) return null;

            await Mediator.Publish(r, CancellationToken.None);

            var md5s = await Facade.Executables.ListMd5Async(
                r.Problem.RunScript,
                r.Problem.CompareScript,
                r.Language.CompileScript);

            var tcss = await Facade.Testcases.ListAsync(r.Problem.Id);

            return new NextJudging(r, md5s, tcss);
        }
    }
}
