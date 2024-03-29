﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using Xunit;
using Xylab.Polygon;
using Xylab.Polygon.Entities;
using Xylab.Polygon.Judgement.Daemon.Fake;
using Xylab.Polygon.Storages;

namespace SatelliteSite.Tests
{
    public class IntegratedServerTests : IClassFixture<WebApplication>
    {
        private readonly WebApplication _factory;

        public IntegratedServerTests(WebApplication factory)
        {
            _factory = factory;
        }

        [Fact]
        public async Task InternalError()
        {
            var judgehost = _factory.Services.GetJudgehost("fake-judgehost-0");

            await _factory.RunScoped(async sp =>
            {
                var list = await sp.GetRequiredService<IJudgehostStore>().ListAsync();
                Assert.DoesNotContain(list, l => l.ServerName == "fake-judgehost-0");

                var iec = await sp.GetRequiredService<IInternalErrorStore>().ListAsync();
                Assert.DoesNotContain(iec, ie => ie.Description == "low on disk space on fake-judgehost-0");
            });

            await judgehost.ManualStartAsync(default);
            await judgehost.Strategy.Semaphore.WaitAsync();
            Assert.False(judgehost.Error);

            await _factory.RunScoped(async sp =>
            {
                var list = await sp.GetRequiredService<IJudgehostStore>().ListAsync();
                var item = Assert.Single(list, l => l.ServerName == "fake-judgehost-0");
                Assert.Equal(judgehost.HostName, item.ServerName);
                Assert.False(item.Active);

                var iec = await sp.GetRequiredService<IInternalErrorStore>().ListAsync();
                Assert.Contains(iec, ie => ie.Description == "low on disk space on fake-judgehost-0");
            });
        }

        [Fact]
        public async Task ImportPackage()
        {
            await _factory.RunScoped(async sp =>
            {
                var stream = typeof(IntegratedServerTests).Assembly
                    .GetManifestResourceStream("Xylab.Polygon.Judgement.Integration.Tests.pkg.zip");
                Assert.NotNull(stream);

                var options = sp.GetRequiredService<IOptions<PolygonOptions>>();
                var import = options.Value.CreateImportProviders(sp, "kattis");
                var probs = await import.ImportAsync(stream, "a-plus-b.zip", "admin");
                stream.Dispose();

                var prob = Assert.Single(probs);
                Assert.Equal("A+B Problem", prob.Title);
            });

            var judgehost1 = _factory.Services.GetJudgehost("fake-judgehost-1");
            var judgehost2 = _factory.Services.GetJudgehost("fake-judgehost-2");
            var judgehost3 = _factory.Services.GetJudgehost("fake-judgehost-3");
            var judgehost4 = _factory.Services.GetJudgehost("fake-judgehost-4");

            await judgehost1.ManualStartAsync(default);
            await Task.Delay(2000);
            await judgehost1.StopAsync(default);
            Assert.False(judgehost1.Error);

            await judgehost2.ManualStartAsync(default);
            await judgehost3.ManualStartAsync(default);
            await judgehost4.ManualStartAsync(default);

            const int totalDelay = 60 * 1000;
            var t1 = Task.Delay(totalDelay);
            var t2 = Task.WhenAll(
                judgehost2.Strategy.Semaphore.WaitAsync(),
                judgehost3.Strategy.Semaphore.WaitAsync(),
                judgehost4.Strategy.Semaphore.WaitAsync());

            if (t1 == await Task.WhenAny(t1, t2))
                throw new System.TimeoutException();

            await judgehost2.StopAsync(default);
            await judgehost3.StopAsync(default);
            await judgehost4.StopAsync(default);
            Assert.False(judgehost2.Error);
            Assert.False(judgehost3.Error);
            Assert.False(judgehost4.Error);

            await _factory.RunScoped(async sp =>
            {
                var langs = sp.GetRequiredService<ILanguageStore>();
                var lang = await langs.FindAsync("cpp");
                Assert.False(lang.AllowJudge);

                var jstore = sp.GetRequiredService<IJudgingStore>();
                var list = await jstore.ListAsync(j => j.Active, 1000);

                Assert.Equal(3, list[0].SubmissionId);
                Assert.Equal(4, list[1].SubmissionId);
                Assert.Equal(1, list[2].SubmissionId);
                Assert.Equal(2, list[3].SubmissionId);

                Assert.Equal(Verdict.WrongAnswer, list[0].Status);
                Assert.Equal(Verdict.TimeLimitExceeded, list[1].Status);
                Assert.Equal(Verdict.Pending, list[2].Status);
                Assert.Equal(Verdict.Accepted, list[3].Status);
            });
        }
    }
}
