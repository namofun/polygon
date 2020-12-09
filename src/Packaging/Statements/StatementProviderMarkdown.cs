using Microsoft.Extensions.FileProviders;
using Polygon.Entities;
using Polygon.Models;
using Polygon.Storages;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Polygon.Packaging
{
    public class MarkdownStatementProvider : IStatementProvider
    {
        public IProblemStore Store { get; }

        public ITestcaseStore Testcases { get; }

        public MarkdownStatementProvider(IProblemStore store, ITestcaseStore testcases)
        {
            Store = store;
            Testcases = testcases;
        }

        private async Task<string> TryReadFileAsync(int problemId, string fileName)
        {
            var fileInfo = await Store.GetFileAsync(problemId, fileName);
            return (await fileInfo.ReadAsync()) ?? string.Empty;
        }

        public async Task<Statement> ReadAsync(Problem problem)
        {
            var description = await TryReadFileAsync(problem.Id, "description.md");
            var inputdesc = await TryReadFileAsync(problem.Id, "inputdesc.md");
            var outputdesc = await TryReadFileAsync(problem.Id, "outputdesc.md");
            var hint = await TryReadFileAsync(problem.Id, "hint.md");
            var interact = await TryReadFileAsync(problem.Id, "interact.md");

            var testcases = await Testcases.ListAsync(problem.Id, false);
            var samples = new List<MemoryTestcase>();

            foreach (var item in testcases)
            {
                var input = item.CustomInput ?? await (await Testcases.GetInputAsync(item)).ReadAsync();
                var output = item.CustomOutput ?? await (await Testcases.GetOutputAsync(item)).ReadAsync();
                if (input == null || output == null)
                    throw new InvalidOperationException($"Input or output invalid for testcase t{item.Id}.");
                samples.Add(new MemoryTestcase(item.Description, input!, output!, item.Point));
            }

            return new Statement(problem, description, inputdesc, outputdesc, hint, interact, string.Empty, samples);
        }
    }
}
