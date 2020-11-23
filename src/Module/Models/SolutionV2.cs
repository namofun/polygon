using Polygon.Entities;
using Polygon.Models;
using System.Collections.Generic;

namespace SatelliteSite.PolygonModule.Models
{
    /// <inheritdoc />
    public class SolutionV2 : Solution
    {
        public bool CombinedRunCompare { get; set; }
        public string CompileError { get; set; }

        public string LanguageName { get; set; }
        public string LanguageFileExtension { get; set; }
        public string ServerName { get; set; }
        public int TestcaseNumber { get; set; }

        public Judging Judging { get; set; }
        public int TimeLimit { get; set; }
        public double TimeFactor { get; set; }

        public IEnumerable<Judging> AllJudgings { get; set; }
        public string SourceCode { get; set; }
        public IEnumerable<(JudgingRun, Testcase)> DetailsV2 { get; set; }
    }
}
