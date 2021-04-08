using Microsoft.AspNetCore.Mvc;
using Polygon.Entities;
using System.Collections.Generic;

namespace Polygon.Models
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

        public string GetRunDetailsUrl(IUrlHelper urlHelper, int rid);

        public string GetTestcaseUrl(IUrlHelper urlHelper, int rid, string file);

        public string GetRunFileUrl(IUrlHelper urlHelper, int rid, string file);
    }
}
