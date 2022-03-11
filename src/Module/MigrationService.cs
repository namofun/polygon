using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xylab.Polygon.Entities;
using Xylab.Polygon.Storages;

namespace SatelliteSite
{
    public static class PolygonMigrationService
    {
        public static async Task MigratePolygonV1Async(this IHost host)
        {
            using var scope = host.Services.CreateScope();
            var facade = scope.ServiceProvider.GetRequiredService<IPolygonFacade>();

            bool firstRun = true;
            do
            {
                var jp = await facade.Judgings.FindAsync(
                    predicate: j => j.PolygonVersion == 0,
                    selector: j => new { j.Id, j.SubmissionId, j.Status, j.s.ProblemId });
                if (jp == null) break;

                if (firstRun)
                {
                    host.Services.GetRequiredService<ILoggerFactory>()
                        .CreateLogger("SatelliteSite.PolygonModule.MigrationService")
                        .LogWarning("Start to migrate to polygon V1. This may take several minutes.");
                    firstRun = false;
                }

                if (jp.Status == Verdict.Pending)
                {
                    await facade.Judgings.UpdateAsync(
                        id: jp.Id,
                        j => new Judging { PolygonVersion = 1 });
                }
                else if (jp.Status == Verdict.CompileError)
                {
                    await facade.Judgings.UpdateAsync(
                        id: jp.Id,
                        j => new Judging { PolygonVersion = 1, RunVerdicts = "" });
                }
                else
                {
                    var c = await facade.Judgings.GetDetailsAsync(
                        problemId: jp.ProblemId,
                        judgingId: jp.Id,
                        selector: (tc, jr) => new { Verdict = (Verdict?)jr.Status });

                    var line = new string(c
                        .Reverse()
                        .SkipWhile(a => !a.Verdict.HasValue)
                        .Reverse()
                        .Select(a => Xylab.Polygon.ResourceDictionary.ConvertToChar(a.Verdict))
                        .ToArray());

                    await facade.Judgings.UpdateAsync(
                        id: jp.Id,
                        j => new Judging { PolygonVersion = 1, RunVerdicts = line });
                }
            }
            while (true);
        }

        public static IHost MigratePolygonV1(this IHost host)
        {
            MigratePolygonV1Async(host).Wait();
            return host;
        }
    }
}
