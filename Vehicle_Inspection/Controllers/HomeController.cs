using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Vehicle_Inspection.Models;
using Vehicle_Inspection.Data;

namespace Vehicle_Inspection.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly VehInsContext _context;

        public HomeController(ILogger<HomeController> logger, VehInsContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            // Pass Roles data to ViewData
            ViewData["Roles"] = _context.Roles.ToList();
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
