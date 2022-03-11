using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using Xylab.Polygon.Entities;

namespace Xylab.Polygon.Models
{
    public interface ISubmissionDetail
    {
        public int SubmissionId { get; }

        public double RealTimeLimit { get; }

        public int JudgingId { get; }

        public ICollection<Judging> AllJudgings { get; }

        public IEnumerable<(JudgingRun, Testcase)> DetailsV2 { get; }

        public Verdict Verdict { get; }

        public string CompileError { get; }

        public bool CombinedRunCompare { get; }

        public Judging Judging { get; }

        public string ServerName { get; }

        public string GetRunDetailsUrl(IUrlHelper urlHelper, int runid);

        public string GetTestcaseUrl(IUrlHelper urlHelper, int runid, string file);

        public string GetRunFileUrl(IUrlHelper urlHelper, int runid, string file);
    }
}
