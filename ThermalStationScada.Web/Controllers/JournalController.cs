using Microsoft.AspNetCore.Mvc;
using ThermalStationScada.Web.Data;

namespace ThermalStationScada.Web.Controllers;

public class JournalController : Controller  
{
    private readonly ApplicationDbContext _db;

    public JournalController(ApplicationDbContext db)
    {
        _db = db;
    }

    public IActionResult Index()
    {
        List<LoginEvent> events = _db.LoginEvents
            .OrderByDescending(e => e.Id)
            .Take(100)
            .ToList();

        return View(events);
    }
}