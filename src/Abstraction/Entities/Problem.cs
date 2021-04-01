namespace Polygon.Entities
{
    /// <summary>
    /// The entity class for problems.
    /// </summary>
    public class Problem
    {
        /// <summary>
        /// The problem ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The title of problem
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// The source of problem
        /// </summary>
        /// <remarks>If multiple, separate them with commas.</remarks>
        public string Source { get; set; }

        /// <summary>
        /// The tag of problem
        /// </summary>
        /// <remarks>If multiple, separate them with commas.</remarks>
        public string TagName { get; set; }

        /// <summary>
        /// Whether to allow judgement of this problem
        /// </summary>
        public bool AllowJudge { get; set; }

        /// <summary>
        /// Whether to allow submission of this problem
        /// </summary>
        public bool AllowSubmit { get; set; }

        /// <summary>
        /// The time limit for testcase
        /// </summary>
        /// <remarks>The unit of time limit is <c>ms</c>.</remarks>
        public int TimeLimit { get; set; } = 1000;

        /// <summary>
        /// The memory limit for testcase
        /// </summary>
        /// <remarks>The unit of memory limit is <c>KB</c>.</remarks>
        public int MemoryLimit { get; set; } = 524288;

        /// <summary>
        /// The output limit for testcase
        /// </summary>
        /// <remarks>The unit of output limit is <c>KB</c>.</remarks>
        public int OutputLimit { get; set; } = 4096;

        /// <summary>
        /// The run script for running submissions
        /// </summary>
        /// <remarks>The <see cref="Executable.Type"/> should be <c>run</c>.</remarks>
        public string RunScript { get; set; }

        /// <summary>
        /// The compare script for comparing submissions output
        /// </summary>
        /// <remarks>The <see cref="Executable.Type"/> should be <c>compare</c>.</remarks>
        public string CompareScript { get; set; }

        /// <summary>
        /// The argument passed to compare script
        /// </summary>
        public string? CompareArguments { get; set; }

        /// <summary>
        /// Whether to use the same script for run and compare
        /// </summary>
        /// <remarks>If <c>true</c>, this is likely an interactive problem.</remarks>
        public bool CombinedRunCompare { get; set; }

        /// <summary>
        /// Whether to share the testcase data
        /// </summary>
        public bool Shared { get; set; }

#pragma warning disable CS8618
        /// <summary>
        /// Construct an empty problem for querying from database.
        /// </summary>
        public Problem()
        {
        }
#pragma warning restore CS8618
    }
}
