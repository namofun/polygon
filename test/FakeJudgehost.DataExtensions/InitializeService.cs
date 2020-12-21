using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using SatelliteSite.IdentityModule.Services;
using System;
using System.Threading.Tasks;

namespace Polygon.FakeJudgehost
{
    internal class InitializeFakeJudgehostService : IInitializeFakeJudgehostService
    {
        private bool _finished;
        private readonly IServiceProvider _service;

        public InitializeFakeJudgehostService(IServiceProvider service)
        {
            _service = service;
        }

        private async Task ExecuteAsync()
        {
            using var scope = _service.CreateScope();
            var options = _service.GetRequiredService<IOptions<DaemonOptions>>();
            var sp = scope.ServiceProvider;

            var userManager = sp.GetRequiredService<IUserManager>();
            var newUser = userManager.CreateEmpty(options.Value.UserName);
            newUser.Email = "fake-test@fake-test.com";
            newUser.EmailConfirmed = true;
            await userManager.CreateAsync(newUser, options.Value.Password);
            await userManager.AddToRoleAsync(newUser, "Judgehost");
            _finished = true;
        }

        public Task EnsureAsync()
        {
            if (!_finished)
                return ExecuteAsync();
            return Task.CompletedTask;
        }
    }
}
