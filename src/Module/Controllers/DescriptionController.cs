using Microsoft.AspNetCore.Mvc;
using Polygon.Packaging;
using Polygon.Storages;
using SatelliteSite.PolygonModule.Models;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.PolygonModule.Controllers
{
    [Area("Polygon")]
    [Route("[area]/{pid}/[controller]/[action]")]
    public class DescriptionController : PolygonControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Preview(bool @new = false)
        {
            ViewBag.Title = @new ? "Preview" : "View";
            ViewBag.Current = @new ? "Preview" : "Public";
            ViewBag.Id = Problem.Id;
            ViewBag.Content = @new
                ? HttpContext.GetService<IStatementWriter>().BuildHtml(await StatementAsync())
                : (await ReadFileAsync("view.html") ?? string.Empty);
            return View();
        }


        [HttpGet("{target}")]
        public async Task<IActionResult> Markdown(string target)
        {
            if (!StorageExtensions.MarkdownFiles.Contains(target))
                return NotFound();

            var lastVersion = await ReadFileAsync($"{target}.md") ?? "";
            ViewBag.ProblemId = Problem.Id;

            return View(new MarkdownModel
            {
                Markdown = lastVersion,
                BackingStore = "p" + Problem.Id,
                Target = target
            });
        }


        [HttpPost("{target}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Markdown(string target, MarkdownModel model)
        {
            if (!StorageExtensions.MarkdownFiles.Contains(target))
                return NotFound();
            if (target != model.Target || $"p{Problem.Id}" != model.BackingStore)
                return BadRequest();
            model.Markdown ??= string.Empty;

            await Facade.Problems.WriteFileAsync(Problem, $"{target}.md", model.Markdown);
            StatusMessage = "Description saved.";
            return RedirectToAction();
        }


        [HttpGet]
        public async Task<IActionResult> Generate(
            [FromServices] IStatementWriter writer)
        {
            var content = writer.BuildHtml(await StatementAsync());
            await Facade.Problems.WriteFileAsync(Problem, "view.html", content);
            StatusMessage = "Problem description saved successfully.";
            return RedirectToAction(nameof(Preview), new { @new = false });
        }


        [HttpGet]
        public async Task<IActionResult> GenerateLatex(
            [FromServices] IStatementWriter writer)
        {
            var memstream = new MemoryStream();
            using (var zip = new ZipArchive(memstream, ZipArchiveMode.Create, true))
                writer.BuildLatex(zip, await StatementAsync());
            memstream.Position = 0;
            return File(memstream, "application/zip", $"p{Problem.Id}-statements.zip");
        }
    }
}
