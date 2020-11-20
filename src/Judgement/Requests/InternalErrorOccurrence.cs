using MediatR;
using Polygon.Entities;
using Polygon.Models;
using System.ComponentModel.DataAnnotations;

namespace Polygon.Judgement
{
    public class InternalErrorOccurrence : IRequest<(InternalError, InternalErrorDisable)>
    {
        /// <summary>
        /// The description of the internal error
        /// </summary>
        [Required]
        public string description { get; set; }

        /// <summary>
        /// The log of the judgehost
        /// </summary>
        [Required]
        public string judgehostlog { get; set; }

        /// <summary>
        /// The object to disable in JSON format
        /// </summary>
        [Required]
        public string disabled { get; set; }

        /// <summary>
        /// The contest ID associated with this internal error
        /// </summary>
        public int? cid { get; set; }

        /// <summary>
        /// The ID of the judging that was being worked on
        /// </summary>
        public int? judgingid { get; set; }


#pragma warning disable CS8618
        public InternalErrorOccurrence()
        {
        }
#pragma warning restore CS8618
    }
}
