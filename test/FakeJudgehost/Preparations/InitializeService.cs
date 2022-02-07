using System.Threading.Tasks;

namespace Polygon.FakeJudgehost
{
    internal interface IInitializeFakeJudgehostService
    {
        Task EnsureAsync();
    }
}
