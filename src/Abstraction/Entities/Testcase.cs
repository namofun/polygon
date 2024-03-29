﻿namespace Xylab.Polygon.Entities
{
    /// <summary>
    /// The entity class for testcases.
    /// </summary>
    public class Testcase
    {
        /// <summary>
        /// The testcase ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// The problem ID
        /// </summary>
        public int ProblemId { get; set; }

        /// <summary>
        /// Whether this testcase is secret
        /// </summary>
        /// <remarks>If <c>true</c>, this is not shown in description. Otherwise, this is a sample testcase.</remarks>
        public bool IsSecret { get; set; }

        /// <summary>
        /// The MD5 for the input content
        /// </summary>
        public string Md5sumInput { get; set; }

        /// <summary>
        /// The MD5 for the output content
        /// </summary>
        public string Md5sumOutput { get; set; }

        /// <summary>
        /// The length for the input content
        /// </summary>
        public int InputLength { get; set; }

        /// <summary>
        /// The length for the output content
        /// </summary>
        public int OutputLength { get; set; }

        /// <summary>
        /// The rank of testcase
        /// </summary>
        public int Rank { get; set; }

        /// <summary>
        /// The score of testcase
        /// </summary>
        public int Point { get; set; }

        /// <summary>
        /// The description of testcase
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The custom display input
        /// </summary>
        public string? CustomInput { get; set; }

        /// <summary>
        /// The custom display output
        /// </summary>
        public string? CustomOutput { get; set; }

#pragma warning disable CS8618
        /// <summary>
        /// Construct an empty testcase for querying from database.
        /// </summary>
        public Testcase()
        {
        }
#pragma warning restore CS8618
    }
}
