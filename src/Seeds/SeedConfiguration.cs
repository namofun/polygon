using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Polygon.Entities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Polygon.Storages
{
    public class SeedConfiguration<TContext> :
        EntityTypeConfigurationSupplier<TContext>,
        IEntityTypeConfiguration<Executable>,
        IEntityTypeConfiguration<Language>
        where TContext : DbContext
    {
        private static Language NL(
            string langid,
            string externalid,
            string name,
            string extensions,
            int require_entry_point,
            string entry_point_description,
            int allow_submit,
            int allow_judge,
            double time_factor,
            string compile_script)
        {
            return new Language
            {
                TimeFactor = time_factor,
                Name = name,
                Id = langid,
                CompileScript = compile_script,
                AllowJudge = allow_judge == 1,
                AllowSubmit = allow_submit == 1,
                FileExtension = extensions.AsJson<string[]>().Single(),
            };
        }

        public static List<Language> GetSeedLanguages()
        {
            const string NULL = null;

            return new List<Language>
            {
                NL("adb", "ada", "Ada", "[\"adb\"]", 0, NULL, 0, 1, 1, "adb"),
                NL("awk", "awk", "AWK", "[\"awk\"]", 0, NULL, 0, 1, 1, "awk"),
                NL("bash", "bash", "Bash shell", "[\"bash\"]", 0, "Main file", 0, 1, 1, "bash"),
                NL("c", "c", "C", "[\"c\"]", 0, NULL, 1, 1, 1, "c"),
                NL("cpp", "cpp", "C++", "[\"cpp\"]", 0, NULL, 1, 1, 1, "cpp"),
                NL("csharp", "csharp", "C#", "[\"cs\"]", 0, NULL, 0, 1, 1, "csharp"),
                NL("f95", "f95", "Fortran", "[\"f95\"]", 0, NULL, 0, 1, 1, "f95"),
                NL("hs", "haskell", "Haskell", "[\"hs\"]", 0, NULL, 0, 1, 1, "hs"),
                NL("java", "java", "Java", "[\"java\"]", 0, "Main class", 1, 1, 1, "java_javac_detect"),
                NL("js", "javascript", "JavaScript", "[\"js\"]", 0, "Main file", 0, 1, 1, "js"),
                NL("lua", "lua", "Lua", "[\"lua\"]", 0, NULL, 0, 1, 1, "lua"),
                NL("kt", "kotlin", "Kotlin", "[\"kt\"]", 1, "Main class", 0, 1, 1, "kt"),
                NL("pas", "pascal", "Pascal", "[\"pas\"]", 0, "Main file", 0, 1, 1, "pas"),
                NL("pl", "pl", "Perl", "[\"pl\"]", 0, "Main file", 0, 1, 1, "pl"),
                NL("plg", "prolog", "Prolog", "[\"plg\"]", 0, "Main file", 0, 1, 1, "plg"),
                NL("py2", "python2", "Python 2", "[\"py\"]", 0, "Main file", 1, 1, 1, "py2"),
                NL("py3", "python3", "Python 3", "[\"py\"]", 0, "Main file", 1, 1, 1, "py3"),
                NL("r", "r", "R", "[\"R\"]", 0, "Main file", 0, 1, 1, "r"),
                NL("rb", "ruby", "Ruby", "[\"rb\"]", 0, "Main file", 0, 1, 1, "rb"),
                NL("scala", "scala", "Scala", "[\"scala\"]", 0, NULL, 0, 1, 1, "scala"),
                NL("sh", "sh", "POSIX shell", "[\"sh\"]", 0, "Main file", 0, 1, 1, "sh"),
                NL("swift", "swift", "Swift", "[\"swift\"]", 0, "Main file", 0, 1, 1, "swift"),
            };
        }

        public void Configure(EntityTypeBuilder<Language> entity)
        {
            var more = GetSeedLanguages();
            ClearUp(more, "Id", e => e.Id, entity.Metadata);
            entity.HasData(more);
        }

        public static List<Executable> GetSeedExecutables()
        {
            var executables = new List<Executable>();

            const string prefix = "Polygon.Seeds.Executables.";
            var assembly = typeof(SeedConfiguration<TContext>).Assembly;
            foreach (var fileName in assembly.GetManifestResourceNames())
            {
                if (!fileName.StartsWith(prefix)) continue;
                using var stream = assembly.GetManifestResourceStream(fileName);
                var count = new byte[stream.Length];
                int len2 = stream.Read(count, 0, count.Length);
                if (len2 != count.Length) throw new IndexOutOfRangeException();
                var file = fileName[prefix.Length..(fileName.Length - 4)];
                var type = file;
                var description = $"default {file} script";

                if (file.StartsWith("compile."))
                {
                    type = "compile";
                    file = file[8..];
                    if (file == "java_javac")
                        description = "compiler for java";
                    else if (file == "java_javac_detect")
                        description = "compiler for java with class name detect";
                    else
                        description = "compiler for " + file;
                }

                executables.Add(new Executable
                {
                    Id = file,
                    Description = description,
                    Md5sum = count.ToMD5().ToHexDigest(true),
                    ZipSize = count.Length,
                    ZipFile = count,
                    Type = type,
                });
            }

            return executables;
        }

        public void Configure(EntityTypeBuilder<Executable> entity)
        {
            var more = GetSeedExecutables();
            ClearUp(more, "Id", e => e.Id, entity.Metadata);
            entity.HasData(more);
        }

        public static void ClearUp<T, TId>(
            List<T> more,
            string idColumn,
            Func<T, TId> idSelector,
            Microsoft.EntityFrameworkCore.Metadata.IMutableEntityType entityType)
        {
            var existing = entityType.GetSeedData()
                .Select(e => e[idColumn])
                .Cast<TId>()
                .ToList();

            for (int i = 0; i < more.Count; i++)
            {
                if (existing.Contains(idSelector(more[i])))
                {
                    more.RemoveAt(i);
                    i--;
                }
            }
        }
    }
}
