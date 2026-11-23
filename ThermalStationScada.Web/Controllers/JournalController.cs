using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ThermalStationScada.Web.Data;
using ThermalStationScada.Web.Security;

namespace ThermalStationScada.Web.Controllers;

[Authorize(Roles = AppRoles.Admin)]
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