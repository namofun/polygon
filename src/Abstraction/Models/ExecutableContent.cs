namespace Polygon.Models
{
    /// <summary>
    /// The model class for executable contents.
    /// </summary>
    public class ExecutableContent
    {
        /// <summary>
        /// The file name of source file
        /// </summary>
        public string FileName { get; set; }

        /// <summary>
        /// The file content of source file
        /// </summary>
        public string FileContent { get; set; }

        /// <summary>
        /// The permission flags of source file
        /// </summary>
        public int Flags { get; set; }

        /// <summary>
        /// Infer the file name for source file.
        /// </summary>
        /// <returns>The inferred file name.</returns>
        public string GetDummyFileName()
        {
            string dummyFileName = FileName;

            if (!System.IO.Path.HasExtension(dummyFileName) &&
                (FileContent.StartsWith("#!/bin/sh") || FileContent.StartsWith("#!/bin/bash")))
            {
                dummyFileName += ".sh";
            }

            return dummyFileName;
        }

#pragma warning disable CS8618
        /// <summary>
        /// Construct an empty content.
        /// </summary>
        public ExecutableContent()
        {
        }
#pragma warning restore CS8618
    }
}
