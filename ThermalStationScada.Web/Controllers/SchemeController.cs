using Microsoft.AspNetCore.Mvc;
using ThermalStationScada.Web.Services;

namespace ThermalStationScada.Web.Controllers
{
    public class SchemeController : Controller
    {
        private readonly LiveValuesCache _cache;
        private readonly SchemeConfigService _scheme;

        public SchemeController(LiveValuesCache cache, SchemeConfigService configService)
        {
            _cache = cache;
            _scheme = configService;
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
    }
}
