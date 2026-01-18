using Microsoft.AspNetCore.Mvc;
using Vehicle_Inspection.Service;

namespace Vehicle_Inspection.Controllers
{
    public class TollController : Controller
    {
        private readonly ITollService _tollService;

        public TollController(ITollService tollService)
        {
            _tollService = tollService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult GetInspections(string? search, int? status)
        {
            var inspections = _tollService.GetInspections(search, status);
            return Json(inspections);
        }
    }
}
