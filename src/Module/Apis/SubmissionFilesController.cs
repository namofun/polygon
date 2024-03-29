﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;
using Xylab.Polygon.Models;
using Xylab.Polygon.Storages;

namespace SatelliteSite.PolygonModule.Apis
{
    /// <summary>
    /// The controller for fetching submission files.
    /// </summary>
    [Area("Api")]
    [Route("[area]/contests/{cid}/submissions")]
    [AuthenticateWithAllSchemes]
    [Authorize(Roles = "Administrator,Judgehost,CDS")]
    public class SubmissionFilesController : ApiControllerBase
    {
        /// <summary>
        /// Get the files for the given submission as a ZIP archive
        /// </summary>
        /// <param name="submitid">The ID of the entity to get</param>
        /// <param name="cid">The contest ID</param>
        /// <param name="store"></param>
        /// <response code="200">The files for the submission as a ZIP archive</response>
        /// <response code="500">An error occurred while creating the ZIP file</response>
        [HttpGet("{submitid}/[action]")]
        [Produces("application/zip")]
        public async Task<IActionResult> Files(
            [FromRoute] int cid,
            [FromRoute] int submitid,
            [FromServices] ISubmissionStore store)
        {
            var src = await store.GetFileAsync(submitid);
            if (src == null) return NotFound();

            var srcDecoded = Convert.FromBase64String(src.SourceCode);
            var memStream = new MemoryStream();

            using (var zip = new ZipArchive(memStream, ZipArchiveMode.Create, true))
                zip.CreateEntryFromByteArray(srcDecoded, src.FileName);
            memStream.Position = 0;
            return File(memStream, "application/zip");
        }


        /// <summary>
        /// Get the source code of all the files for the given submission
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <param name="submitid">The ID of the entity to get</param>
        /// <param name="store"></param>
        /// <response code="200">The files for the submission</response>
        [HttpGet("{submitid}/[action]")]
        [Produces("application/json")]
        public async Task<ActionResult<SubmissionFile[]>> SourceCode(
            [FromRoute] int cid,
            [FromRoute] int submitid,
            [FromServices] ISubmissionStore store)
        {
            var src = await store.GetFileAsync(submitid);
            if (src == null) return NotFound();
            return new[] { src };
        }


        /// <summary>
        /// Get the plain source code of the only file for the given submission
        /// </summary>
        /// <param name="cid">The contest ID</param>
        /// <param name="submitid">The ID of the entity to get</param>
        /// <param name="store"></param>
        /// <response code="200">The files for the submission</response>
        [HttpGet("{submitid}/[action]")]
        [Produces("application/octet-stream")]
        public async Task<IActionResult> SourcePlain(
            [FromRoute] int cid,
            [FromRoute] int submitid,
            [FromServices] ISubmissionStore store)
        {
            var src = await store.GetFileAsync(submitid);
            if (src == null) return NotFound();
            var decode = Convert.FromBase64String(src.SourceCode);
            return File(decode, "application/octet-stream", src.FileName);
        }
    }
}
