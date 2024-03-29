﻿using MediatR;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.IdentityModule.Models;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xylab.Polygon.Storages;

namespace SatelliteSite.PolygonModule
{
    public class ProblemAuthorOfAddition : IAdditionalRole
    {
        private readonly int probid;

        public string Category => "Author of";

        public string Title { get; }

        public string Text { get; }

        public string GetUrl(object urlHelper)
        {
            var url = urlHelper as IUrlHelper;
            return url.Action("Overview", "Editor", new { area = "Polygon", probid });
        }

        public ProblemAuthorOfAddition((int, string) item)
        {
            Title = item.Item2;
            probid = item.Item1;
            Text = "p" + item.Item1;
        }
    }

    public class PolygonAdditionProvider : INotificationHandler<UserDetailModel>
    {
        private readonly IProblemStore _store;

        public PolygonAdditionProvider(IProblemStore store)
            => _store = store;

        public async Task Handle(UserDetailModel notification, CancellationToken cancellationToken)
        {
            var list = await _store.ListPermissionAsync(notification.User.Id);
            notification.AddMore(list.Select(a => new ProblemAuthorOfAddition(a)).ToList());
        }
    }
}
