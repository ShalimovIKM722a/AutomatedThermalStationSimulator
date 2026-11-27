using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThermalStationScada.Web.Data;
using ThermalStationScada.Web.Services;
namespace ThermalStationScada.Web.Controllers;


[Authorize]
public class SchemeController : Controller
{
    private readonly LiveValuesCache _cache;
    private readonly ActuatorStateCache _actuatorCache;
    private readonly SchemeConfigService _scheme;
    private readonly ApplicationDbContext _db;
    public SchemeController(LiveValuesCache cache, ActuatorStateCache actuatorCache, SchemeConfigService
    scheme, ApplicationDbContext db)
    {
        _cache = cache;
        _scheme = scheme;
        _db = db;
        _actuatorCache = actuatorCache;
    }
    public IActionResult Index()
    {
        ViewBag.Actuators = _scheme.GetActuators();
        return View(_scheme.GetSensors());
    }
    [HttpGet]
    public IActionResult Current()
    {
        return Json(_cache.GetAll());
    }

    [HttpPost]
    [Authorize(Policy = "CanReadSavings")]
    public async Task<IActionResult> Save()
    {
        string userName = User.Identity?.Name ?? "unknown";
        foreach (var sensor in _cache.GetAll())
        {
            _db.SensorSamples.Add(new SensorSample
            {
                Topic = sensor.Topic,
                Value = sensor.Value,
                Unit = sensor.Unit,
                MeasuredAtUtc = sensor.MeasuredAtUtc,
                SavedBy = userName
            });
        }

        await _db.SaveChangesAsync();

        return RedirectToAction("History");
    }

    [HttpGet]
    public IActionResult Actuators()
    {
        return Json(_actuatorCache.GetAll());
    }


    public async Task<IActionResult> History()
    {
        var samples = await _db.SensorSamples.OrderByDescending(s => s.Id)
                                             .Take(50)
                                             .ToListAsync();

        return View(samples);
    }
}
