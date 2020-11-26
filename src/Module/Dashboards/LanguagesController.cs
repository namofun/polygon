using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Polygon.Entities;
using Polygon.Storages;
using SatelliteSite.PolygonModule.Models;
using System.Linq;
using System.Threading.Tasks;

namespace SatelliteSite.PolygonModule.Dashboards
{
    [Area("Dashboard")]
    [Authorize(Roles = "Administrator")]
    [Route("[area]/[controller]")]
    [AuditPoint(Entities.AuditlogType.Language)]
    public class LanguagesController : ViewControllerBase
    {
        private IPolygonFacade Facade { get; }
        private ILanguageStore Store => Facade.Languages;
        public LanguagesController(IPolygonFacade facade) => Facade = facade;


        [HttpGet]
        public async Task<IActionResult> List()
        {
            return View(await Store.ListAsync());
        }


        [HttpGet("{langid}")]
        public async Task<IActionResult> Detail(string langid)
        {
            var lang = await Store.FindAsync(langid);
            if (lang is null) return NotFound();

            var subs = await Facade.Submissions.ListWithJudgingAsync(
                pagination: (1, 100),
                predicate: s => s.Language == langid);

            var maxSub = subs.Select(s => s.SubmissionId).Append(-1).Max();
            var minSub = subs.Select(s => s.SubmissionId).Append(-1).Min();
            ViewBag.Authors = await Facade.Submissions.GetAuthorNamesAsync(
                sids: s => s.Language == langid && s.Id >= minSub && s.Id <= maxSub);

            ViewBag.Problems = await Facade.Problems.ListNameAsync(
                s => s.Language == langid && s.Id >= minSub && s.Id <= maxSub);

            ViewBag.Submissions = subs;
            return View(lang);
        }


        [HttpPost("{langid}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleSubmit(string langid)
        {
            var lang = await Store.FindAsync(langid);
            if (lang is null) return NotFound();

            await Store.ToggleSubmitAsync(langid, !lang.AllowSubmit);
            await HttpContext.AuditAsync("toggle allow submit", langid);
            return RedirectToAction(nameof(Detail), new { langid });
        }


        [HttpPost("{langid}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleJudge(string langid)
        {
            var lang = await Store.FindAsync(langid);
            if (lang is null) return NotFound();

            await Store.ToggleJudgeAsync(langid, !lang.AllowJudge);
            await HttpContext.AuditAsync("toggle allow judge", langid);
            return RedirectToAction(nameof(Detail), new { langid });
        }


        [HttpGet("{langid}/[action]")]
        public async Task<IActionResult> Edit(string langid)
        {
            var lang = await Store.FindAsync(langid);
            if (lang == null) return NotFound();
            ViewBag.Executables = await Facade.Executables.ListAsync("compile");

            ViewBag.Operator = "Edit";
            return View(new LanguageEditModel
            {
                CompileScript = lang.CompileScript,
                ExternalId = lang.Id,
                FileExtension = lang.FileExtension,
                Name = lang.Name,
                TimeFactor = lang.TimeFactor,
            });
        }


        [HttpPost("{langid}/[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string langid, LanguageEditModel model)
        {
            var lang = await Store.FindAsync(langid);
            if (lang == null) return NotFound();

            lang.CompileScript = model.CompileScript;
            lang.FileExtension = model.FileExtension;
            lang.TimeFactor = model.TimeFactor;
            lang.Name = model.Name;

            await Store.UpdateAsync(lang);
            await HttpContext.AuditAsync("updated", langid);
            return RedirectToAction(nameof(Detail), new { langid });
        }


        [HttpGet("[action]")]
        public async Task<IActionResult> Add()
        {
            ViewBag.Executables = await Facade.Executables.ListAsync("compile");
            ViewBag.Operator = "Add";
            return View("Edit", new LanguageEditModel { TimeFactor = 1 });
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Add(LanguageEditModel model)
        {
            var entity = await Store.CreateAsync(new Language
            {
                CompileScript = model.CompileScript,
                FileExtension = model.FileExtension,
                AllowJudge = false,
                AllowSubmit = false,
                Id = model.ExternalId,
                Name = model.Name,
                TimeFactor = model.TimeFactor,
            });

            await HttpContext.AuditAsync("added", entity.Id);
            return RedirectToAction(nameof(Detail), new { langid = entity.Id });
        }
    }
}
