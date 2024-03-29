﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Threading.Tasks;
using Xylab.Polygon.Storages;

namespace SatelliteSite.PolygonModule.Dashboards
{
    [Area("Dashboard")]
    [Authorize(Roles = "Administrator")]
    [Route("[area]/[controller]")]
    [AuditPoint(AuditlogType.Judgehost)]
    public class JudgehostsController : ViewControllerBase
    {
        private IJudgehostStore Store { get; }
        public JudgehostsController(IJudgehostStore store) => Store = store;


        [HttpGet]
        public async Task<IActionResult> List(
            [FromServices] IMemoryCache memoryCache)
        {
            ViewBag.Load = await memoryCache
                .GetOrCreateAsync("judgehost_loads", async entry =>
                {
                    var result = await Store.LoadAsync();
                    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(30);
                    return result;
                });

            return View(await Store.ListAsync());
        }


        [HttpGet("{hostname}")]
        public async Task<IActionResult> Detail(string hostname,
            [FromServices] IJudgingStore judgings)
        {
            var host = await Store.FindAsync(hostname);
            if (host is null) return NotFound();
            ViewBag.Host = host;
            ViewBag.Count = await judgings.CountAsync(j => j.Server == hostname);
            ViewBag.Judgings = await judgings.ListAsync(j => j.Server == hostname, j => j, 100);
            return View();
        }


        [HttpGet("{hostname}/{tobe}")]
        public async Task<IActionResult> Toggle(string hostname, string tobe)
        {
            bool active = tobe == "activate";
            if (!active && tobe != "deactivate") return NotFound();

            var affected = await Store.ToggleAsync(hostname, active);
            if (affected == 0) return NotFound();

            await HttpContext.AuditAsync($"mark {(active ? "" : "in")}active", hostname);
            return RedirectToAction(nameof(List));
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateAll()
        {
            await Store.ToggleAsync(null, true);
            await HttpContext.AuditAsync("marked all active", null);
            return RedirectToAction(nameof(List));
        }


        [HttpPost("[action]")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateAll()
        {
            await Store.ToggleAsync(null, false);
            await HttpContext.AuditAsync("marked all inactive", null);
            return RedirectToAction(nameof(List));
        }
    }
}
