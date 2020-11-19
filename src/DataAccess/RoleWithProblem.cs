using SatelliteSite.IdentityModule.Services;

namespace Polygon.Storages
{
    public interface IRoleWithProblem : IRole
    {
        public int? ProblemId { get; }
    }
}
