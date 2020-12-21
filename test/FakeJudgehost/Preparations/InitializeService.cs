using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Polygon.FakeJudgehost.DataExtensions")]
namespace Polygon.FakeJudgehost
{
    internal interface IInitializeFakeJudgehostService
    {
        Task EnsureAsync();
    }
}
