﻿using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;
using Xylab.Polygon.Entities;
using Xylab.Polygon.Storages;

namespace Xylab.Polygon.Packaging
{
    public sealed class DataImportProvider : ImportProviderBase
    {
        public DataImportProvider(
            IPolygonFacade facade,
            ILogger<DataImportProvider> logger)
            : base(facade, logger)
        {
        }

        public override async Task<List<Problem>> ImportAsync(
            Stream stream,
            string uploadFileName,
            string username)
        {
            using ZipArchive zipArchive = new(stream);

            ImportContext ctx = await CreateAsync(new Problem
            {
                AllowJudge = false,
                AllowSubmit = false,
                Title = TryGetPackageName(uploadFileName),
                CompareScript = "compare",
                RunScript = "run",
                MemoryLimit = 524288,
                OutputLimit = 4096,
                Source = username,
                TimeLimit = 10000,
            });

            await ctx.KattisAsync(zipArchive, string.Empty, true);

            foreach (var a in zipArchive.Entries.Where(
                a => !a.Name.EndsWith(".in")
                  && !a.Name.EndsWith(".out")
                  && !a.Name.EndsWith(".desc")
                  && !a.Name.EndsWith(".point")))
            {
                Log($"Has file {a.FullName}.");
            }

            var problem = await ctx.FinalizeAsync();
            return new List<Problem> { problem };
        }
    }
}
