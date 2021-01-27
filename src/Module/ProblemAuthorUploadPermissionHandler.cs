using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Polygon.Storages;
using SatelliteSite.Substrate.Dashboards;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SatelliteSite.PolygonModule
{
    public class ProblemAuthorUploadPermissionHandler : INotificationHandler<ImageUploadPermission>
    {
        public async Task Handle(ImageUploadPermission notification, CancellationToken cancellationToken)
        {
            if (notification.Type != "p") return;

            if (!(notification.Id is int probid) ||
                !int.TryParse(notification.Context.User.GetUserId(), out int userid))
            {
                notification.Handled = false;
                return;
            }

            if (notification.Context.User.IsInRole("Administrator"))
            {
                notification.Handled = true;
                return;
            }

            var store = notification.Context.RequestServices.GetRequiredService<IProblemStore>();
            notification.Handled = await store.CheckPermissionAsync(probid, userid);
        }
    }
}
