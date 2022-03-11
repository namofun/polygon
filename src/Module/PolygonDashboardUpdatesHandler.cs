using MediatR;
using Microsoft.Extensions.Caching.Memory;
using SatelliteSite.Substrate.Dashboards;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xylab.Polygon.Storages;

namespace SatelliteSite.PolygonModule
{
    public class PolygonDashboardUpdatesHandler : INotificationHandler<DashboardUpdates>
    {
        IJudgehostStore Judgehosts { get; }

        IInternalErrorStore InternalErrors { get; }

        IMemoryCache Cache { get; }

        public PolygonDashboardUpdatesHandler(
            IJudgehostStore judgehostStore,
            IInternalErrorStore internalErrorStore,
            IMemoryCache memoryCache)
        {
            Judgehosts = judgehostStore;
            InternalErrors = internalErrorStore;
            Cache = memoryCache;
        }

        public async Task Handle(
            DashboardUpdates notification,
            CancellationToken cancellationToken)
        {
            var result = await Cache.GetOrCreateAsync("polygon_dashboard_updates", async entry =>
            {
                int judgehosts = await Judgehosts.CountFailureAsync();
                int internal_error = await InternalErrors.CountOpenAsync();
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(5);
                return (judgehosts, internal_error);
            });

            notification.Add(nameof(result.judgehosts), result.judgehosts);
            notification.Add(nameof(result.internal_error), result.internal_error);
        }
    }
}
