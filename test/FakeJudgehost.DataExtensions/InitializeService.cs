using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
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

            IUser? user = await userManager.FindByNameAsync(options.Value.UserName);
            if (user != null)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                await userManager.ResetPasswordAsync(user, token, options.Value.Password);
                if (!await userManager.IsInRoleAsync(user, "Judgehost"))
                {
                    await userManager.AddToRoleAsync(user, "Judgehost");
                }
            }
            else
            {
                user = userManager.CreateEmpty(options.Value.UserName);
                user.Email = options.Value.UserName + "@acm.xylab.fun";
                user.EmailConfirmed = false;
                await userManager.CreateAsync(user, options.Value.Password);
                await userManager.AddToRoleAsync(user, "Judgehost");
            }

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
