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

        Task<IBlobInfo> GetStatementFileAsync(int problemId, string target);

        Task<IBlobInfo> WriteStatementFileAsync(int problemId, string target, string content);
    }

    public delegate string JudgingOutputNameFormat(int judgingId, int runId, string type);
    public delegate string TestcaseNameFormat(int problemId, int testcaseId, string target);
    public delegate string StatementNameFormat(int problemId, string target);

    public class PolygonFileProvider : IProblemFileProvider, IJudgingFileProvider
    {
        private readonly IBlobProvider _blobProvider;
        private readonly JudgingOutputNameFormat _judgingOutputNameFormat;
        private readonly TestcaseNameFormat _testcaseNameFormat;
        private readonly StatementNameFormat _statementNameFormat;

        public PolygonFileProvider(
            IBlobProvider blobProvider,
            JudgingOutputNameFormat? jonFormatter = null,
            TestcaseNameFormat? tnFormatter = null,
            StatementNameFormat? snFormatter = null)
        {
            _blobProvider = blobProvider;
            _judgingOutputNameFormat = jonFormatter ?? DefaultJudgingOutputNameFormat;
            _testcaseNameFormat = tnFormatter ?? DefaultTestcaseNameFormat;
            _statementNameFormat = snFormatter ?? DefaultStatementNameFormat;
        }

        public static string DefaultJudgingOutputNameFormat(int judgingId, int runId, string type)
        {
            return $"j{judgingId}/r{runId}.{type}";
        }

        public static string DefaultTestcaseNameFormat(int problemId, int testcaseId, string target)
        {
            return $"p{problemId}/t{testcaseId}.{target}";
        }

        public static string DefaultStatementNameFormat(int problemId, string target)
        {
            return $"p{problemId}/{target}";
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

        public Task<IBlobInfo> GetStatementFileAsync(int problemId, string target)
        {
            return _blobProvider.GetFileInfoAsync(_statementNameFormat(problemId, target));
        }

        public Task<IBlobInfo> WriteStatementFileAsync(int problemId, string target, string content)
        {
            return _blobProvider.WriteStringAsync(_statementNameFormat(problemId, target), content);
        }
    }
}
