namespace Polygon.Models
{
    /// <summary>
    /// The load for judgehosts.
    /// </summary>
    public class JudgehostLoad
    {
        /// <summary>
        /// The host name of judgehost
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// The running seconds
        /// </summary>
        public double Load { get; set; }

        /// <summary>
        /// The load type
        /// </summary>
        public int Type { get; set; }

#pragma warning disable CS8618
        /// <summary>
        /// Construct a <see cref="JudgehostLoad"/>.
        /// </summary>
        public JudgehostLoad()
        {
        }
#pragma warning restore CS8618
    }
}
