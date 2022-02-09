using Microsoft.Extensions.FileProviders;
using System.IO;
using System.Threading.Tasks;

namespace Polygon.Storages
{
    public interface IJudgingFileProvider
    {
        Task<IBlobInfo> GetJudgingRunOutputAsync(int judgingId, int runId, string type);

        Task<IBlobInfo> WriteJudgingRunOutputAsync(int judgingId, int runId, string type, byte[] content);
    }

    public interface IProblemFileProvider
    {
        Task<IBlobInfo> GetTestcaseFileAsync(int problemId, int testcaseId, string target);

        Task<IBlobInfo> WriteTestcaseFileAsync(int problemId, int testcaseId, string target, Stream source);

        Task<IBlobInfo> GetStatementAsync(int problemId);

        Task<IBlobInfo> WriteStatementAsync(int problemId, string content);

        Task<IBlobInfo> GetStatementSectionAsync(int problemId, string section);

        Task<IBlobInfo> WriteStatementSectionAsync(int problemId, string section, string content);
    }

    public delegate string JudgingOutputNameFormat(int judgingId, int runId, string type);
    public delegate string TestcaseNameFormat(int problemId, int testcaseId, string target);
    public delegate string StatementNameFormat(int problemId);
    public delegate string StatementSectionNameFormat(int problemId, string section);

    public class PolygonFileProvider : IProblemFileProvider, IJudgingFileProvider
    {
        private readonly IBlobProvider _blobProvider;
        private readonly JudgingOutputNameFormat _judgingOutputNameFormat;
        private readonly TestcaseNameFormat _testcaseNameFormat;
        private readonly StatementNameFormat _statementNameFormat;
        private readonly StatementSectionNameFormat _statementSectionNameFormat;

        public PolygonFileProvider(
            IBlobProvider blobProvider,
            JudgingOutputNameFormat? jonFormatter = null,
            TestcaseNameFormat? tnFormatter = null,
            StatementNameFormat? snFormatter = null,
            StatementSectionNameFormat? ssnFormatter = null)
        {
            _blobProvider = blobProvider;
            _judgingOutputNameFormat = jonFormatter ?? DefaultJudgingOutputNameFormat;
            _testcaseNameFormat = tnFormatter ?? DefaultTestcaseNameFormat;
            _statementNameFormat = snFormatter ?? DefaultStatementNameFormat;
            _statementSectionNameFormat = ssnFormatter ?? DefaultStatementSectionNameFormat;
        }

        public static string DefaultJudgingOutputNameFormat(int judgingId, int runId, string type)
        {
            return $"j{judgingId}/r{runId}.{type}";
        }

        public static string DefaultTestcaseNameFormat(int problemId, int testcaseId, string target)
        {
            return $"p{problemId}/t{testcaseId}.{target}";
        }

        public static string DefaultStatementNameFormat(int problemId)
        {
            return $"p{problemId}/view.html";
        }

        public static string DefaultStatementSectionNameFormat(int problemId, string section)
        {
            return $"p{problemId}/{section}.md";
        }

        public Task<IBlobInfo> GetJudgingRunOutputAsync(int judgingId, int runId, string type)
        {
            return _blobProvider.GetFileInfoAsync(_judgingOutputNameFormat(judgingId, runId, type));
        }

        public Task<IBlobInfo> WriteJudgingRunOutputAsync(int judgingId, int runId, string type, byte[] content)
        {
            return _blobProvider.WriteBinaryAsync(_judgingOutputNameFormat(judgingId, runId, type), content);
        }

        public Task<IBlobInfo> GetTestcaseFileAsync(int problemId, int testcaseId, string target)
        {
            return _blobProvider.GetFileInfoAsync(_testcaseNameFormat(problemId, testcaseId, target));
        }

        public Task<IBlobInfo> WriteTestcaseFileAsync(int problemId, int testcaseId, string target, Stream source)
        {
            return _blobProvider.WriteStreamAsync(_testcaseNameFormat(problemId, testcaseId, target), source);
        }

        public Task<IBlobInfo> GetStatementAsync(int problemId)
        {
            return _blobProvider.GetFileInfoAsync(_statementNameFormat(problemId));
        }

        public Task<IBlobInfo> WriteStatementAsync(int problemId, string content)
        {
            return _blobProvider.WriteStringAsync(_statementNameFormat(problemId), content);
        }

        public Task<IBlobInfo> GetStatementSectionAsync(int problemId, string section)
        {
            return _blobProvider.GetFileInfoAsync(_statementSectionNameFormat(problemId, section));
        }

        public Task<IBlobInfo> WriteStatementSectionAsync(int problemId, string section, string content)
        {
            return _blobProvider.WriteStringAsync(_statementSectionNameFormat(problemId, section), content);
        }
    }
}
