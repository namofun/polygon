using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xylab.Polygon.Entities;
using Xylab.Polygon.Models;
using Xylab.Polygon.Storages;

namespace Xylab.Polygon.Packaging
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

        private async Task<string> TryReadSectionAsync(int problemId, string sectionName)
        {
            var fileInfo = await Store.GetStatementSectionAsync(problemId, sectionName);
            return (await fileInfo.ReadAsStringAsync()) ?? string.Empty;
        }

        public async Task<Statement> ReadAsync(Problem problem)
        {
            var description = await TryReadSectionAsync(problem.Id, "description");
            var inputdesc = await TryReadSectionAsync(problem.Id, "inputdesc");
            var outputdesc = await TryReadSectionAsync(problem.Id, "outputdesc");
            var hint = await TryReadSectionAsync(problem.Id, "hint");
            var interact = await TryReadSectionAsync(problem.Id, "interact");

            var testcases = await Testcases.ListAsync(problem.Id, false);
            var samples = new List<MemoryTestcase>();

            foreach (var item in testcases)
            {
                var input = item.CustomInput ?? await (await Testcases.GetInputAsync(item)).ReadAsStringAsync();
                var output = item.CustomOutput ?? await (await Testcases.GetOutputAsync(item)).ReadAsStringAsync();
                if (input == null || output == null)
                {
                    throw new InvalidOperationException($"Input or output invalid for testcase t{item.Id}.");
                }

                samples.Add(new MemoryTestcase(item.Description, input!, output!, item.Point));
            }

            return new Statement(problem, description, inputdesc, outputdesc, hint, interact, string.Empty, samples);
        }
    }
}
