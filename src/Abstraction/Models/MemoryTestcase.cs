namespace Xylab.Polygon.Models
{
    /// <summary>
    /// The model class for testcases in memory.
    /// </summary>
    public class MemoryTestcase
    {
        /// <summary>
        /// The description text
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The input content
        /// </summary>
        public string Input { get; set; }
        
        /// <summary>
        /// The output content
        /// </summary>
        public string Output { get; set; }

        /// <summary>
        /// The score of testcase
        /// </summary>
        public int Point { get; set; }

        /// <summary>
        /// Initialize an empty memory testcase.
        /// </summary>
        /// <param name="desc">The description.</param>
        /// <param name="input">The input data.</param>
        /// <param name="output">The output data.</param>
        /// <param name="point">The score.</param>
        public MemoryTestcase(string desc, string input, string output, int point)
        {
            Description = desc;
            Input = input.Replace("\r\n", "\n").Replace("\r", "");
            Output = output.Replace("\r\n", "\n").Replace("\r", "");
            Point = point;
        }
    }
}
