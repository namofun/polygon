using Polygon.Entities;
using System.Collections.Generic;

namespace Polygon.Models
{
    /// <summary>
    /// The model class for problem statements.
    /// </summary>
    public class Statement
    {
        /// <summary>
        /// The legend description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// The input description
        /// </summary>
        public string Input { get; }

        /// <summary>
        /// The output description
        /// </summary>
        public string Output { get; }

        /// <summary>
        /// The hint and explanation
        /// </summary>
        public string Hint { get; }

        /// <summary>
        /// The interaction protocol
        /// </summary>
        public string Interaction { get; }

        /// <summary>
        /// The problem entity
        /// </summary>
        public Problem Problem { get; }

        /// <summary>
        /// The problem shortname
        /// </summary>
        public string ShortName { get; }

        /// <summary>
        /// The sample testcases
        /// </summary>
        public IReadOnlyList<MemoryTestcase> Samples { get; }

        /// <summary>
        /// Construct statement for processing export.
        /// </summary>
        /// <param name="prob">The problem entity.</param>
        /// <param name="desc">The legend description.</param>
        /// <param name="input">The input description.</param>
        /// <param name="output">The output description.</param>
        /// <param name="hint">The hint.</param>
        /// <param name="interaction">The interaction protocol.</param>
        /// <param name="shortname">The short name.</param>
        /// <param name="samples">The samples.</param>
        public Statement(Problem prob, string desc, string input, string output, string hint, string interaction, string shortname, IReadOnlyList<MemoryTestcase> samples)
        {
            Problem = prob;
            Description = desc;
            Input = input;
            Output = output;
            Hint = hint;
            Interaction = interaction;
            ShortName = shortname;
            Samples = samples;
        }
    }
}
