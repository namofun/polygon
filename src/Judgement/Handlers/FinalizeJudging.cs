using Polygon.Entities;
using Polygon.Events;
using System;
using System.Threading.Tasks;

namespace Polygon.Judgement
{
    public partial class DOMjudgeLikeHandlers
    {
        /// <summary>
        /// Finish the judging pipe, persist into database,
        /// create the events and notify scoreboard.
        /// </summary>
        private async Task FinalizeJudging(JudgingFinishedEvent e)
        {
            await Facade.Judgings.UpdateAsync(e.Judging);

            Telemetry.TrackDependency(
                dependencyTypeName: "JudgeHost",
                dependencyName: e.Judging.Server!,
                data: $"j{e.Judging.Id} judged " + e.Judging.Status,
                startTime: e.Judging.StartTime ?? DateTimeOffset.Now,
                duration: (e.Judging.StopTime - e.Judging.StartTime) ?? TimeSpan.Zero,
                success: e.Judging.Status != Verdict.UndefinedError);

            await Mediator.Publish(e);
        }
    }
}
