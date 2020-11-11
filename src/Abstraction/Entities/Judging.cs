using System;
using System.Collections.Generic;

namespace Polygon.Entities
{
    /// <summary>
    /// The entity class for judgings.
    /// </summary>
    public class Judging
    {
        /// <summary>
        /// The judging ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The submission ID
        /// </summary>
        public int SubmissionId { get; set; }

        /// <summary>
        /// Whether this judging is valid active result
        /// </summary>
        public bool Active { get; set; }

        /// <summary>
        /// Whether to take a full test
        /// </summary>
        public bool FullTest { get; set; }

        /// <summary>
        /// The start time of this judging
        /// </summary>
        public DateTimeOffset? StartTime { get; set; }

        /// <summary>
        /// The end time of this judging
        /// </summary>
        public DateTimeOffset? StopTime { get; set; }

        /// <summary>
        /// The judgehost that take this judging
        /// </summary>
        public string? Server { get; set; }

        /// <summary>
        /// The judging verdict
        /// </summary>
        public Verdict Status { get; set; }

        /// <summary>
        /// 执行时间，以ms为单位
        /// </summary>
        public int? ExecuteTime { get; set; }

        /// <summary>
        /// 执行内存，以kb为单位
        /// </summary>
        public int? ExecuteMemory { get; set; }

        /// <summary>
        /// The output of compiler
        /// </summary>
        /// <remarks>This field is base64 encoded.</remarks>
        public string? CompileError { get; set; }

        /// <summary>
        /// 重测请求编号
        /// </summary>
        public int? RejudgeId { get; set; }

        /// <summary>
        /// 重测时前一个活跃评测编号
        /// </summary>
        public int? PreviousJudgingId { get; set; }

        /// <summary>
        /// 评测点分数总和
        /// </summary>
        public int? TotalScore { get; set; }

        /// <summary>
        /// 评测结果导航属性
        /// </summary>
        public ICollection<Detail> Details { get; set; }

        /// <summary>
        /// The navigation to submission
        /// </summary>
        public Submission s { get; set; }

#pragma warning disable CS8618
        /// <summary>
        /// Construct an empty judging for querying from database.
        /// </summary>
        public Judging()
        {
        }
#pragma warning restore CS8618
    }
}
