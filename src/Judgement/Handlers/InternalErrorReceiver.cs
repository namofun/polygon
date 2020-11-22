using MediatR;
using Polygon.Entities;
using Polygon.Models;
using Polygon.Storages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Polygon.Judgement
{
    public partial class DOMjudgeLikeHandlers : IRequestHandler<InternalErrorOccurrence, (InternalError, InternalErrorDisable)>
    {
        public async Task<(InternalError, InternalErrorDisable)> Handle(InternalErrorOccurrence model, CancellationToken cancellationToken)
        {
            var toDisable = model.Disabled.AsJson<InternalErrorDisable>();
            var kind = toDisable.Kind;

            var ie = await Facade.InternalErrors.CreateAsync(
                new InternalError
                {
                    JudgehostLog = model.JudgehostLog,
                    JudgingId = model.JudgingId,
                    ContestId = model.ContestId,
                    Description = model.Description,
                    Disabled = model.Disabled,
                    Status = InternalErrorStatus.Open,
                    Time = DateTimeOffset.Now,
                });

            if (kind == "language")
            {
                var langid = toDisable.Language!;
                await Facade.Languages.ToggleJudgeAsync(langid, false);

                Telemetry.TrackDependency(
                    dependencyTypeName: "Language",
                    dependencyName: langid,
                    data: model.Description,
                    startTime: DateTimeOffset.Now,
                    duration: TimeSpan.Zero,
                    success: false);
            }
            else if (kind == "judgehost")
            {
                var hostname = toDisable.HostName!;
                await Facade.Judgehosts.ToggleAsync(hostname, false);

                Telemetry.TrackDependency(
                    dependencyTypeName: "JudgeHost",
                    dependencyName: hostname,
                    data: model.Description,
                    startTime: DateTimeOffset.Now,
                    duration: TimeSpan.Zero,
                    success: false);
            }
            else if (kind == "problem")
            {
                var probid = toDisable.ProblemId!.Value;
                await Facade.Problems.ToggleJudgeAsync(probid, false);

                Telemetry.TrackDependency(
                    dependencyTypeName: "Problem",
                    dependencyName: $"p{probid}",
                    data: model.Description,
                    startTime: DateTimeOffset.Now,
                    duration: TimeSpan.Zero,
                    success: false);
            }
            else
            {
                Telemetry.TrackDependency(
                    dependencyTypeName: "Unresolved",
                    dependencyName: kind,
                    data: model.Description,
                    startTime: DateTimeOffset.Now,
                    duration: TimeSpan.Zero,
                    success: false);
            }

            if (model.JudgingId.HasValue)
            {
                var (j, p, c, u, t) = await Facade.Judgings.FindAsync(model.JudgingId.Value);
                if (j == null) throw new InvalidOperationException("Unknown Error Occurred.");
                var request = new ReturnToQueueRequest(j, c, p, u, t);
                await Handle(request, cancellationToken);
            }

            return (ie, toDisable);
        }
    }
}
