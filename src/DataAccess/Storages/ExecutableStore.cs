﻿using Microsoft.EntityFrameworkCore;
using Polygon.Entities;
using Polygon.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Polygon.Storages
{
    public partial class PolygonFacade<TUser, TRole, TContext> : IExecutableStore
    {
        public DbSet<Executable> Executables => Context.Set<Executable>();

        Task<Executable> IExecutableStore.CreateAsync(Executable entity) => CreateEntityAsync(entity);

        Task IExecutableStore.DeleteAsync(Executable entity) => DeleteEntityAsync(entity);

        Task IExecutableStore.UpdateAsync(Executable entity) => UpdateEntityAsync(entity);

        Task IExecutableStore.UpdateAsync(string id, Expression<Func<Executable, Executable>> expression)
        {
            return Executables
                .Where(e => e.Id == id)
                .BatchUpdateAsync(expression);
        }

        Task<Executable> IExecutableStore.FindAsync(string execid)
        {
            return Executables
                .Where(e => e.Id == execid)
                .SingleOrDefaultAsync();
        }

        Task<List<Executable>> IExecutableStore.ListAsync(string? type)
        {
            return Executables
                .WhereIf(type != null, e => e.Type == type)
                .Select(e => new Executable(e.Id, e.Md5sum, e.ZipSize, e.Description, e.Type))
                .ToListAsync();
        }

        Task<Dictionary<string, string>> IExecutableStore.ListMd5Async(params string[] targets)
        {
            targets = (targets ?? Array.Empty<string>()).Distinct().ToArray();
            return Executables
                .Where(e => targets.Contains(e.Id))
                .Select(e => new { e.Id, e.Md5sum })
                .ToDictionaryAsync(e => e.Id, e => e.Md5sum);
        }

        async Task<IReadOnlyList<ExecutableContent>> IExecutableStore.FetchContentAsync(Executable executable)
        {
            var items = new List<ExecutableContent>();
            using var stream = new MemoryStream(executable.ZipFile!, false);
            using var zipArchive = new ZipArchive(stream);

            foreach (var entry in zipArchive.Entries)
            {
                var fileName = entry.FullName;
                var fileExt = Path.GetExtension(fileName);
                fileExt = string.IsNullOrEmpty(fileExt) ? "dummy.sh" : "dummy" + fileExt;

                using var entryStream = entry.Open();
                using var reader = new StreamReader(entryStream, Encoding.UTF8, false);
                var fileContent2 = await reader.ReadToEndAsync();

                items.Add(new ExecutableContent
                {
                    FileName = fileName,
                    FileContent = fileContent2,
                    Flags = entry.ExternalAttributes,
                });
            }

            return items;
        }

        async Task<ILookup<string, string>> IExecutableStore.ListUsageAsync(string execid)
        {
            var compile = await Context.Set<Language>()
                .Where(l => l.CompileScript == execid)
                .Select(l => new { l.Id, Type = "compile" })
                .ToListAsync();
            var run = await Context.Set<Problem>()
                .Where(p => p.RunScript == execid)
                .Select(p => new { Id = p.Id.ToString(), Type = "run" })
                .ToListAsync();
            var compare = await Context.Set<Problem>()
                .Where(p => p.CompareScript == execid)
                .Select(p => new { Id = p.Id.ToString(), Type = "compare" })
                .ToListAsync();
            var query = compile.Concat(run).Concat(compare);
            return query.ToLookup(k => k.Type, v => v.Id);
        }
    }
}