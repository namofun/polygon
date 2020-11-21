using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace Polygon.Packaging
{
    public static class ResourcesDictionary
    {
        public static Stream Read(string name)
        {
            name = "Polygon.Packaging.Resources." + name;
            var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(name);
            if (stream == null) throw new InvalidOperationException($"EmbeddedResource {name} not found.");
            return stream;
        }

        public static IReadOnlyDictionary<string, (string, Func<IServiceProvider, IImportProvider>)> ImportProviders { get; }
            = new Dictionary<string, (string, Func<IServiceProvider, IImportProvider>)>
            {
                ["kattis"] = ("Kattis Package", s => s.GetRequiredService<KattisImportProvider>()),
                ["xysxml"] = ("XiaoYang's XML", s => s.GetRequiredService<XmlImportProvider>()),
                ["hustoj"] = ("HUSTOJ FPS XML", s => s.GetRequiredService<FpsImportProvider>()),
                ["cfplyg"] = ("CodeForces Polygon (Linux)", s => s.GetRequiredService<CodeforcesImportProvider>()),
                ["data"] = ("Data (.in and .out/.ans)", s => s.GetRequiredService<DataImportProvider>()),
            };

        public static IServiceCollection AddPolygonPackaging(this IServiceCollection services)
        {
            services.AddScoped<IExportProvider, KattisExportProvider>();
            services.AddScoped<IStatementProvider, MarkdownStatementProvider>();
            services.AddScoped<IStatementWriter, MarkdownStatementWriter>();

            services.AddScoped<CodeforcesImportProvider>();
            services.AddScoped<DataImportProvider>();
            services.AddScoped<FpsImportProvider>();
            services.AddScoped<KattisImportProvider>();
            services.AddScoped<XmlImportProvider>();
            
            return services;
        }

        public static Stream GetTestlib() => Read("testlib.h");
        public static Stream GetOlymp() => Read("olymp.sty");
        public static Stream GetContestTexBegin() => Read("contest.tex");
    }
}
