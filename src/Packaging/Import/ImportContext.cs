using Microsoft.Extensions.FileProviders;
using Polygon.Entities;
using Polygon.Models;
using Polygon.Storages;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polygon.Packaging
{
    public class ImportContext
    {
        public IPolygonFacade Facade { get; }

        public Action<string> Log { get; }

        public Problem Problem { get; }

        public int Id => Problem.Id;

        public int TestcaseRank { get; private set; }

        public int Flag { get; set; }

        private ImportContext(IPolygonFacade facade, Action<string> log, Problem problem)
        {
            Facade = facade;
            Log = log;
            Problem = problem;
        }

        public async Task KattisAsync(ZipArchive zip, string prefix, bool isSecret)
        {
            if (!string.IsNullOrEmpty(prefix) && !prefix.EndsWith('/'))
                prefix += '/';

            var fileNames = zip.Entries
                .Where(z => z.FullName.StartsWith(prefix) && !z.FullName.EndsWith('/'))
                .Select(z => Path.GetFileNameWithoutExtension(z.FullName[prefix.Length..]))
                .Distinct()
                .ToList();

            fileNames.Sort((x, y) =>
            {
                // check with prefix numbers
                int lenA = 0, lenB = 0;
                for (; lenA < x.Length; lenA++)
                    if (x[lenA] > '9' || x[lenA] < '0') break;
                for (; lenB < y.Length; lenB++)
                    if (y[lenB] > '9' || y[lenB] < '0') break;
                if (lenA == 0 || lenB == 0)
                    return x.CompareTo(y);
                if (lenA != lenB)
                    return lenA.CompareTo(lenB);
                return x.CompareTo(y);
            });


            foreach (var file in fileNames)
            {
                var inp = zip.GetEntry(prefix + file + ".in");
                var outp = zip.GetEntry(prefix + file + ".ans");
                var desc = zip.GetEntry(prefix + file + ".desc");
                var point = zip.GetEntry(prefix + file + ".point");

                string usedParts = "in,out";
                if (outp == null)
                    outp = zip.GetEntry(prefix + file + ".out");
                else
                    usedParts = "in,ans";

                if (inp == null || outp == null)
                {
                    Log($"Ignoring {prefix}{file}.*");
                    continue;
                }

                string descp = file;
                if (desc != null)
                {
                    var content = await desc.ReadAsStringAsync();
                    descp = string.IsNullOrWhiteSpace(content) ? file : content.Trim();
                    usedParts += ",desc";
                }

                int pnt = 0;
                if (point != null)
                {
                    var content = await point.ReadAsStringAsync();
                    int.TryParse(content.Trim(), out pnt);
                    usedParts += ",point";
                }

                await AddAsync(
                    input: (() => inp.Open(), inp.Length),
                    output: (() => outp.Open(), outp.Length),
                    descp, pnt, isSecret, $"{prefix}{file}.{{{usedParts}}}");
            }
        }

        public static async Task<ImportContext> CreateAsync(IPolygonFacade facade, Action<string> log, Problem problem)
        {
            problem.TagName ??= string.Empty;
            problem.Source ??= string.Empty;
            problem = await facade.Problems.CreateAsync(problem);
            log($"Problem p{problem.Id} created.");
            return new ImportContext(facade, log, problem);
        }

        public async Task<Testcase> AddAsync((Func<Stream>, long) input, (Func<Stream>, long) output, string desc, int point, bool isSecret, string from)
        {
            var tc = await Facade.Testcases.CreateAsync(
                inputFactory: input.Item1,
                inputLength: input.Item2,
                outputFactory: output.Item1,
                outputLength: output.Item2,
                entity: new Testcase
                {
                    IsSecret = isSecret,
                    Point = point,
                    Rank = ++TestcaseRank,
                    Description = desc,
                    ProblemId = Problem.Id,
                });

            Log($"Adding testcase t{tc.Id} '{from}'.");
            return tc;
        }

        public async Task<Testcase> AddAsync(MemoryTestcase test, bool isSecret)
        {
            var input = Encoding.UTF8.GetBytes(test.Input);
            var output = Encoding.UTF8.GetBytes(test.Output);
            return await AddAsync(
                input: (() => new MemoryStream(input), input.Length),
                output: (() => new MemoryStream(output), output.Length),
                test.Description, test.Point, isSecret, "memory");
        }

        public Task<Submission> SubmitAsync(string code, string language, Verdict? expected = null)
        {
            return Facade.Submissions.CreateAsync(
                code: code,
                language: language,
                problemId: Problem.Id,
                contestId: null,
                teamId: 0,
                ipAddr: System.Net.IPAddress.Parse("127.0.0.1"),
                via: "polygon-upload",
                username: "SYSTEM",
                expected: expected);
        }

        public Task<IFileInfo> WriteAsync(string fileName, string content)
        {
            return Facade.Problems.WriteFileAsync(Problem, fileName, content);
        }

        public Task<Executable> AddAsync(Executable executable)
        {
            return Facade.Executables.CreateAsync(executable);
        }

        public async Task<Problem> FinalizeAsync()
        {
            Problem.AllowJudge = true;
            await Facade.Problems.CommitChangesAsync(Problem);
            return Problem;
        }
    }
}
