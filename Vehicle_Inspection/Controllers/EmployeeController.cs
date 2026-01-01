using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vehicle_Inspection.Data;
using Vehicle_Inspection.Models;
using System.Linq;

namespace Vehicle_Inspection.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly VehInsContext _context;

        public EmployeeController(VehInsContext context)
        {
            _context = context;
        }

        public IActionResult Index(string search, int? position, string gender, bool isActive, string sort)
        {
            var employees = _context.Users
                .Include(u => u.Position)
                .Include(u => u.Team)
                .AsQueryable();

            // Search
            if (!string.IsNullOrEmpty(search))
            {
                employees = employees.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
            }

            // Filter
            if (position.HasValue)
            {
                employees = employees.Where(u => u.PositionId == position);
            }

            if (!string.IsNullOrEmpty(gender))
            {
                employees = employees.Where(u => u.Gender == gender);
            }

            if (isActive)
            {
                employees = employees.Where(u => u.IsActive);
            }

            // Sorting
            employees = sort switch
            {
                "FullName" => employees.OrderBy(u => u.FullName),
                "BirthDate" => employees.OrderBy(u => u.BirthDate),
                "CreatedAt" => employees.OrderBy(u => u.CreatedAt),
                _ => employees
            };

            // Pass Positions for filtering
            ViewBag.Positions = _context.Positions.ToList();
            ViewBag.Teams = _context.Teams.ToList();
            return View(employees.ToList());
        }
    }
}