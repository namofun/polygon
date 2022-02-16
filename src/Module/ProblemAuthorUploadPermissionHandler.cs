using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Polygon.Entities;
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

            if (notification.Id is not int probid ||
                !int.TryParse(notification.Context.User.GetUserId(), out int userid))
            {
                notification.Reject();
                return;
            }

            if (!notification.Context.User.IsInRole("Administrator"))
            {
                AuthorLevel? level = await notification.Context.RequestServices
                    .GetRequiredService<IProblemStore>()
                    .CheckPermissionAsync(probid, userid);

                if (!level.HasValue || level.Value < AuthorLevel.Writer)
                {
                    notification.Reject();
                    return;
                }
            }

            notification.Accept("problem");
        }
    }
}
