using SatelliteSite.IdentityModule.Services;

namespace Polygon.Storages
{
    public interface IRoleWithProblem : IRole
    {
        /// <summary>
        /// The problem ID associated with problem related roles
        /// </summary>
        public int? ProblemId { get; set; }
    }
}
