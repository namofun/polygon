namespace Xylab.Polygon.Entities
{
    /// <summary>
    /// The entity class for languages.
    /// </summary>
    public class Language
    {
        /// <summary>
        /// The language ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The formal name of language
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Whether to allow submission in this language
        /// </summary>
        public bool AllowSubmit { get; set; }

        /// <summary>
        /// Whether to allow judgement in this language
        /// </summary>
        public bool AllowJudge { get; set; }

        /// <summary>
        /// The script for compiling submissions in this language
        /// </summary>
        /// <remarks>The <see cref="Executable.Type"/> should be <c>compile</c>.</remarks>
        public string CompileScript { get; set; }

        /// <summary>
        /// The factor for testcase running time
        /// </summary>
        public double TimeFactor { get; set; } = 1;

        /// <summary>
        /// The default file extension
        /// </summary>
        public string FileExtension { get; set; }

#pragma warning disable CS8618
        /// <summary>
        /// Construct an empty language for querying from database.
        /// </summary>
        public Language()
        {
        }
#pragma warning restore CS8618
    }
}
