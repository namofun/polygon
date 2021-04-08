using Microsoft.AspNetCore.Mvc;
using Polygon.Entities;
using Polygon.Models;
using System.Collections.Generic;

namespace SatelliteSite.PolygonModule.Models
{
    /// <inheritdoc />
    public class SolutionV2 : Solution, ISubmissionDetail
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

        public ICollection<Judging> AllJudgings { get; set; }
        public string SourceCode { get; set; }
        public IEnumerable<(JudgingRun, Testcase)> DetailsV2 { get; set; }

        public double RealTimeLimit => TimeLimit * TimeFactor / 1000;

        public virtual string GetRunDetailsUrl(IUrlHelper urlHelper, int runid)
        {
            return urlHelper.Action(
                action: "RunDetails",
                controller: "Submissions",
                values: new
                {
                    area = "Polygon",
                    pid = ProblemId,
                    sid = SubmissionId,
                    judgingid = JudgingId,
                    rid = runid,
                });
        }

        public virtual string GetRunFileUrl(IUrlHelper urlHelper, int runid, string file)
        {
            return urlHelper.Action(
                action: "RunDetails",
                controller: "Submissions",
                values: new
                {
                    area = "Polygon",
                    pid = ProblemId,
                    sid = SubmissionId,
                    judgingid = JudgingId,
                    rid = runid,
                    type = file,
                });
        }

        public virtual string GetTestcaseUrl(IUrlHelper urlHelper, int tcid, string file)
        {
            return urlHelper.Action(
                action: "Fetch",
                controller: "Testcases",
                values: new
                {
                    area = "Polygon",
                    pid = ProblemId,
                    tid = tcid,
                    filetype = file
                });
        }
    }
}
