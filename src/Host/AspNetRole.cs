using Polygon.Storages;
using SatelliteSite.IdentityModule.Entities;

namespace SatelliteSite
{
    /// <summary>
    /// The final role type used by system.
    /// </summary>
    public class AspNetRole : Role, IRoleWithProblem
    {
        /// <summary>
        /// The problem ID associated with problem related roles
        /// </summary>
        public int? ProblemId { get; set; }

        /// <summary>
        /// Initializes a new instance of <see cref="AspNetRole"/>.
        /// </summary>
        public AspNetRole() : base()
        {
        }

        /// <summary>
        /// Initializes a new instance of <see cref="AspNetRole"/>.
        /// </summary>
        public AspNetRole(string roleName) : base(roleName)
        {
        }
    }
}
