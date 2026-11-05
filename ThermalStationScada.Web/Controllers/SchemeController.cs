using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThermalStationScada.Web.Data;
using ThermalStationScada.Web.Services;

namespace ThermalStationScada.Web.Controllers;

public class SchemeController : Controller
{
    private readonly LiveValuesCache _cache;
    private readonly SchemeConfigService _scheme;
    private readonly ApplicationDbContext _db;

    public SchemeController(LiveValuesCache cache, SchemeConfigService configService, ApplicationDbContext db)
    {
        _cache = cache;
        _scheme = configService;
        _db = db;
    }

    public IActionResult Index()
    {
        return View(_scheme.GetSensors());
    }

    [HttpGet]
    public IActionResult Current()
    {
        return Json(_cache.GetAll());
    }

    [HttpPost]
    public async Task<IActionResult> Save()
    {
        foreach (var r in _cache.GetAll())
        {
            _db.SensorSamples.Add(new SensorSample
            {
                Topic = r.Topic,
                Value = r.Value,
                Unit = r.Unit,
                MeasuredAtUtc = r.MeasuredAtUtc
            });
        }
        await _db.SaveChangesAsync();
        return RedirectToAction("History");
    }

    public async Task<IActionResult> History()
    {
        var samples = await _db.SensorSamples
        .OrderByDescending(s => s.Id)
        .Take(50)
        .ToListAsync();
        return View(samples);
    }
}
