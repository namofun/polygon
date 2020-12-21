using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Polygon.FakeJudgehost
{
    /// <summary>
    /// The builder extensions for fake judgehost.
    /// </summary>
    public static class FakeJudgehostBuilderExtensions
    {
        /// <summary>
        /// Creates a random password generator.
        /// </summary>
        /// <returns>The thread-unsafe random password generator.</returns>
        private static Func<string> CreatePasswordGenerator()
        {
            const string passwordSource = "abcdefghjkmnpqrstuvwxyzABCDEFGHJKMNPQRSTUVWXYZ23456789";
            var rng = new Random(unchecked((int)DateTimeOffset.Now.Ticks));
            return () =>
            {
                Span<char> pwd = stackalloc char[8];
                for (int i = 0; i < 8; i++) pwd[i] = passwordSource[rng.Next(passwordSource.Length)];
                return pwd.ToString();
            };
        }

        /// <summary>
        /// Generate random judgehost account and let the startup of fake judgehost finish the account creating.
        /// </summary>
        /// <returns>The builder to chain calls.</returns>
        public static FakeJudgehostBuilder AddFakeAccount(this FakeJudgehostBuilder builder)
        {
            var pwd = CreatePasswordGenerator().Invoke();

            builder.ConfigureJudgeDaemon(options =>
            {
                options.Password = pwd;
                options.UserName = "judgehost-" + pwd[0..4];
            });

            builder.Services.AddSingleton<IInitializeFakeJudgehostService, InitializeFakeJudgehostService>();

            return builder;
        }

        /// <summary>
        /// Add fake item seeds to context.
        /// </summary>
        /// <typeparam name="TContext">The databse context.</typeparam>
        /// <returns>The service collection.</returns>
        public static FakeJudgehostBuilder AddFakeSeeds<TContext>(this FakeJudgehostBuilder builder) where TContext : DbContext
        {
            builder.Services.AddDbModelSupplier<TContext, FakeSeedConfiguration<TContext>>();
            return builder;
        }
    }
}
