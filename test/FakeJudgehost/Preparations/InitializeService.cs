using System.Threading.Tasks;

namespace Xylab.Polygon.Judgement.Daemon.Fake
{
    internal interface IInitializeFakeJudgehostService
    {
        Task EnsureAsync();
    }
}
