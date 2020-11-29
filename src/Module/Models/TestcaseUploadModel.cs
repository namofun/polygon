using Microsoft.AspNetCore.Http;
using System.ComponentModel;

namespace SatelliteSite.PolygonModule.Models
{
    public class TestcaseUploadModel
    {
        [DisplayName("Problem ID")]
        public int ProblemId { get; set; }

        [DisplayName("Secret data, not shown to public")]
        public bool IsSecret { get; set; }

        [DisplayName("Description")]
        public string Description { get; set; }

        [DisplayName("Point")]
        public int Point { get; set; }

        [DisplayName("Input Content")]
        public IFormFile InputContent { get; set; }

        [DisplayName("Output Content")]
        public IFormFile OutputContent { get; set; }

        [DisplayName("Custom Input (only on statements)")]
        public string CustomInput { get; set; }

        [DisplayName("Custom Output (only on statements)")]
        public string CustomOutput { get; set; }
    }
}
