using System;

namespace Xylab.Polygon.Entities
{
    /// <summary>
    /// The entity class for details.
    /// </summary>
    public class JudgingRun
    {
        /// <summary>
        /// The running ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The running result
        /// </summary>
        public Verdict Status { get; set; }

        /// <summary>
        /// The judging ID
        /// </summary>
        public int JudgingId { get; set; }

        /// <summary>
        /// The testcase ID
        /// </summary>
        public int TestcaseId { get; set; }

        /// <summary>
        /// The maximum execution memory size
        /// </summary>
        /// <remarks>The unit of memory is <c>KB</c>.</remarks>
        public int ExecuteMemory { get; set; }

        /// <summary>
        /// The total execution time
        /// </summary>
        /// <remarks>The unit of memory is <c>ms</c>.</remarks>
        public int ExecuteTime { get; set; }

        /// <summary>
        /// The finished time of running
        /// </summary>
        public DateTimeOffset CompleteTime { get; set; }

        /// <summary>
        /// The meta data of running
        /// </summary>
        /// <remarks>This field is base64 encoded.</remarks>
        public string MetaData { get; set; }

        /// <summary>
        /// The judging system output
        /// </summary>
        /// <remarks>This field is base64 encoded.</remarks>
        public string OutputSystem { get; set; }

        /// <summary>
        /// The comparer output
        /// </summary>
        /// <remarks>This field is base64 encoded.</remarks>
        public string OutputDiff { get; set; }

        /// <summary>
        /// The navigation to judging
        /// </summary>
        public Judging j { get; set; }

#pragma warning disable CS8618
        /// <summary>
        /// Construct an empty judging run for inserting into database.
        /// </summary>
        public JudgingRun()
        {
        }
#pragma warning restore CS8618
    }
}
