using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using Vehicle_Inspection.Data;
using Vehicle_Inspection.Models;

namespace Vehicle_Inspection.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly VehInsContext _context;

        public HomeController(ILogger<HomeController> logger, VehInsContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> IndexAsync()
        {
            // Pass Roles data to ViewData
            ViewData["Roles"] = _context.Roles.ToList();
            // Thống kê cơ bản
            var stats = new
            {
                TotalInspections = await _context.Inspections.CountAsync(i => !i.IsDeleted),
                TodayInspections = await _context.Inspections
                    .CountAsync(i => !i.IsDeleted && i.CreatedAt.Date == DateTime.Today),
                CompletedToday = await _context.Inspections
                    .CountAsync(i => !i.IsDeleted &&
                                     i.CompletedAt.HasValue &&
                                     i.CompletedAt.Value.Date == DateTime.Today),
                TotalVehicles = await _context.Vehicles.CountAsync(),
                ActiveEmployees = await _context.Users.CountAsync(u => u.IsActive)
            };

            ViewBag.Stats = stats;
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
