﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SatelliteSite.PolygonModule.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using Xylab.Polygon.Entities;
using Xylab.Polygon.Storages;

namespace SatelliteSite.PolygonModule.Dashboards
{
    [Area("Dashboard")]
    [Authorize(Roles = "Administrator")]
    [Route("[area]/[controller]")]
    [AuditPoint(AuditlogType.Executable)]
    public class ExecutablesController : ViewControllerBase
    {
        private IExecutableStore Store { get; }
        public ExecutablesController(IExecutableStore store) => Store = store;


        [HttpGet]
        public async Task<IActionResult> List()
        {
            return View(await Store.ListAsync());
        }


        [HttpGet("{execid}")]
        public async Task<IActionResult> Detail(string execid)
        {
            var exec = await Store.FindAsync(execid);
            if (exec == null) return NotFound();
            ViewBag.Usage = await Store.ListUsageAsync(exec.Id);
            return View(exec);
        }


        [HttpGet("{execid}/[action]")]
        public async Task<IActionResult> Delete(string execid)
        {
            var exec = await Store.FindAsync(execid);
            if (exec == null) return NotFound();

            return AskPost(
                title: $"Delete executable {execid} - \"{exec.Description}\"",
                message: $"You're about to delete executable {execid} - \"{exec.Description}\".\n" +
                    "Warning, this will create dangling references in languages.\n" +
                    "Are you sure?",
                area: "Dashboard", controller: "Executables", action: "Delete",
                routeValues: new { execid },
                type: BootstrapColor.danger);
        }


        [HttpGet("{execid}/[action]")]
        public async Task<IActionResult> Edit(string execid)
        {
            var exec = await Store.FindAsync(execid);
            if (exec == null) return NotFound();
            
            return View(new ExecutableEditModel
            {
                ExecId = exec.Id,
                Description = exec.Description,
                Type = exec.Type,
            });
        }


        [HttpPost("{execid}/[action]")]
        public async Task<IActionResult> Edit(string execid, ExecutableEditModel model)
        {
            var exec = await Store.FindAsync(execid);
            if (exec == null) return NotFound();
            if (!"compile,compare,run".Split(',').Contains(model.Type))
                return BadRequest();

            if (model.Archive != null)
                (exec.ZipFile, exec.Md5sum) = await model.Archive.ReadAsync();
            exec.ZipSize = exec.ZipFile.Length;
            exec.Description = model.Description ?? execid;
            exec.Type = model.Type;
            await Store.UpdateAsync(exec);
            await HttpContext.AuditAsync("updated", execid);
            return RedirectToAction(nameof(Detail), new { execid });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(ExecutableEditModel model)
        {
            // Validations
            string message = null;
            if (model.Archive == null || model.Archive.Length >= 1 << 20)
                message = "Error in executable uploading.";
            else if (model.ExecId == null)
                message = "No executable name specified.";
            else if (model.Description == null)
                message = "No description specified.";
            else if (!"compile,compare,run".Split(',').Contains(model.Type))
                message = "Error type specified.";

            if (message != null)
            {
                ModelState.AddModelError("EXEC", message);
                return View(model);
            }

            // creation
            var (zip, md5) = await model.Archive.ReadAsync();

            var e = await Store.CreateAsync(new Executable
            {
                Description = model.Description,
                Id = model.ExecId,
                Type = model.Type,
                ZipFile = zip,
                Md5sum = md5,
                ZipSize = zip.Length,
            });

            StatusMessage = $"Executable {e.Id} uploaded successfully.";
            await HttpContext.AuditAsync("created", e.Id);
            return RedirectToAction("Detail", new { execid = e.Id });
        }


        [HttpGet("{execid}/[action]")]
        [ActionName("Content")]
        public async Task<IActionResult> ViewContent(string execid)
        {
            var exec = await Store.FindAsync(execid);
            if (exec is null) return NotFound();
            ViewBag.Executable = exec;
            var items = await Store.FetchContentAsync(exec);
            return View(items);
        }


        [HttpPost("{execid}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(string execid, int inajax)
        {
            var exec = await Store.FindAsync(execid);
            if (exec == null) return NotFound();

            try
            {
                await Store.DeleteAsync(exec);
                StatusMessage = $"Executable {execid} deleted successfully.";
                await HttpContext.AuditAsync("deleted", execid);
            }
            catch
            {
                StatusMessage = $"Error deleting executable {execid}, foreign key constraints failed.";
            }

            return RedirectToAction(nameof(List));
        }


        [HttpGet("{execid}/[action]")]
        public async Task<IActionResult> Download(string execid)
        {
            var bytes = await Store.FindAsync(execid);
            if (bytes is null) return NotFound();
            return File(bytes.ZipFile, "application/zip", $"{execid}.zip", false);
        }
    }
}
