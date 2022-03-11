namespace Xylab.Polygon.Entities
{
    /// <summary>
    /// The entity class for executables.
    /// </summary>
    public class Executable
    {
        /// <summary>
        /// The executable ID
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// The MD5 for the executable zip
        /// </summary>
        /// <remarks>Should be the md5 result of <see cref="ZipFile"/>.</remarks>
        public string Md5sum { get; set; }

        /// <summary>
        /// The executable zip byte array
        /// </summary>
        /// <remarks>The maximum file size is 1MB. When this is model class, this field is <c>null</c>.</remarks>
        public byte[]? ZipFile { get; set; }

        /// <summary>
        /// The size of executable zip (in bytes)
        /// </summary>
        /// <remarks>Should be the length of <see cref="ZipFile"/>.</remarks>
        public int ZipSize { get; set; }

        /// <summary>
        /// The description of executable
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The type of executable
        /// </summary>
        /// <remarks>The value should be <c>compile</c>, <c>compare</c>, or <c>run</c>.</remarks>
        public string Type { get; set; }

#pragma warning disable CS8618
        /// <summary>
        /// Construct an empty executable for inserting into database.
        /// </summary>
        public Executable()
        {
        }
#pragma warning restore CS8618

        /// <summary>
        /// Construct a summary of executable for querying from database.
        /// </summary>
        public Executable(string id, string md5, int size, string description, string type)
        {
            Id = id;
            Description = description;
            Md5sum = md5;
            Type = type;
            ZipSize = size;
        }
    }
}
