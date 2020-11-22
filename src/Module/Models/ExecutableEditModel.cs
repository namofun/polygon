using Microsoft.AspNetCore.Http;

namespace SatelliteSite.PolygonModule.Models
{
    public class ExecutableEditModel
    {
        public string ExecId { get; set; }

        public string Description { get; set; }

        public string Type { get; set; }

        public IFormFile Archive { get; set; }
    }
}
