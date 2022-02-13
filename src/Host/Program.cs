using Azure.Storage.Blobs;
using Markdig;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileProviders.AzureBlob;
using Microsoft.Extensions.Hosting;
using Polygon.FakeJudgehost;
using Polygon.Storages;
using SatelliteSite.IdentityModule.Entities;
using System.IO;

namespace SatelliteSite
{
    public class Program
    {
        public static IHost Current { get; private set; }

        public static void Main(string[] args)
        {
            Current = CreateHostBuilder(args).Build();
            Current.AutoMigrate<DefaultContext>();
            Current.MigratePolygonV1();
            Current.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .MarkDomain<Program>()
                .AddModule<IdentityModule.IdentityModule<User, Role, DefaultContext>>()
                .AddModule<PolygonModule.PolygonModule<Polygon.DefaultRole<DefaultContext, QueryCache<DefaultContext>>>>()
                .AddModule<HostModule>()
                .AddDatabase<DefaultContext>((c, b) => b.UseSqlServer(c.GetConnectionString("UserDbConnection"), b => b.UseBulk()))
                .AddApplicationInsights()
                .ConfigureSubstrateDefaults<DefaultContext>(builder =>
                {
                    builder.ConfigureServices((context, services) =>
                    {
                        services.AddMarkdown();
                        services.AddDbModelSupplier<DefaultContext, PolygonIdentityEntityConfiguration<User, DefaultContext>>();
                        //services.AddDbModelSupplier<DefaultContext, SeedConfiguration<DefaultContext>>();

                        if (bool.Parse(context.Configuration["UseAzureConvergence"]))
                        {
                            BlobServiceClient blobServiceClient = new("UseDevelopmentStorage=true");
                            var runs = blobServiceClient.GetBlobContainerClient("runs");
                            var probs = blobServiceClient.GetBlobContainerClient("problems");
                            var wwwroot = blobServiceClient.GetBlobContainerClient("wwwroot");
                            runs.CreateIfNotExists();
                            probs.CreateIfNotExists();
                            wwwroot.CreateIfNotExists();
                            var runsCache = Path.Combine(context.HostingEnvironment.ContentRootPath, "Runs/.cache");
                            var probsCache = Path.Combine(context.HostingEnvironment.ContentRootPath, "Problems/.cache");
                            var wwwrootCache = Path.Combine(context.HostingEnvironment.ContentRootPath, "wwwroot/.cache");
                            if (!Directory.Exists(runsCache)) Directory.CreateDirectory(runsCache);
                            if (!Directory.Exists(probsCache)) Directory.CreateDirectory(probsCache);
                            if (!Directory.Exists(wwwrootCache)) Directory.CreateDirectory(wwwrootCache);

                            services.ConfigurePolygonStorage(options =>
                            {
                                options.JudgingFileProvider = new AzurePolygonFileProvider(runs, runsCache, false);
                                options.ProblemFileProvider = new AzurePolygonFileProvider(probs, probsCache, false);
                            });

                            IFileProvider physicalWwwroot = context.HostingEnvironment.WebRootFileProvider;
                            IFileProvider blobWwwroot = new AzureWwwrootProvider(wwwroot, wwwrootCache);
                            context.HostingEnvironment.WebRootFileProvider = new CompositeFileProvider(physicalWwwroot, blobWwwroot);
                        }
                        else
                        {
                            services.ConfigurePolygonStorage(options =>
                            {
                                options.JudgingDirectory = Path.Combine(context.HostingEnvironment.ContentRootPath, "Runs");
                                options.ProblemDirectory = Path.Combine(context.HostingEnvironment.ContentRootPath, "Problems");
                            });
                        }

                        /*
                        services.AddFakeJudgehost()
                            .AddJudgehost<FakeJudgeActivity>("judgehost-0")
                            .AddHttpClientFactory(sp =>
                            {
                                return new System.Net.Http.HttpClient()
                                {
                                    BaseAddress = new System.Uri("https://localhost:41359/api/")
                                };
                            });
                        //*/
                    });
                });

        private class AzurePolygonFileProvider : PolygonFileProvider
        {
            public AzurePolygonFileProvider(BlobContainerClient client, string localFileCachePath, bool allowAutoCache)
                : base(new AzureBlobProvider(client, localFileCachePath, default, allowAutoCache))
            {
            }
        }

        private class AzureWwwrootProvider : AzureBlobProvider, IWwwrootFileProvider
        {
            public AzureWwwrootProvider(BlobContainerClient client, string localFileCachePath)
                : base(client, localFileCachePath, default, default, new[] { "/images/problem" })
            {
            }
        }
    }
}
