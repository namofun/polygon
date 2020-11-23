using System.ComponentModel;

namespace SatelliteSite.PolygonModule.Models
{
    public class CodeSubmitModel
    {
        [DisplayName("Language")]
        public string Language { get; set; }

        [DisplayName("Source Code")]
        public string Code { get; set; }
    }
}
