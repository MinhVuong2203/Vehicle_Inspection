using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vehicle_Inspection.Data;
using Vehicle_Inspection.Service;

namespace Vehicle_Inspection.Controllers
{
    public class DecentralizeController : Controller
    {
        private readonly VehInsContext _context;
        public DecentralizeController(VehInsContext context)
        {
            _context = context;
           
        }
        public IActionResult Index(string search, int? position, string gender, bool? isActive, string sort)
        {
            var employees = _context.Users
                .Include(u => u.Position)
                .Include(u => u.Team)
                .AsQueryable();

            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                employees = employees.Where(u =>
                    u.FullName.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search) ||
                    u.Phone.Contains(search));
            }

            if (position.HasValue)
            {
                employees = employees.Where(u => u.PositionId == position);
            }

            if (!string.IsNullOrEmpty(gender))
            {
                employees = employees.Where(u => u.Gender == gender);
            }

            if (isActive.HasValue)
            {
                employees = employees.Where(u => u.IsActive == isActive.Value);
            }

            employees = sort switch
            {
                "FullName" => employees.OrderBy(u => u.FullName),
                "FullName_desc" => employees.OrderByDescending(u => u.FullName),
                "BirthDate" => employees.OrderBy(u => u.BirthDate),
                "BirthDate_desc" => employees.OrderByDescending(u => u.BirthDate),
                "CreatedAt" => employees.OrderBy(u => u.CreatedAt),
                "CreatedAt_desc" => employees.OrderByDescending(u => u.CreatedAt),
                _ => employees.OrderByDescending(u => u.CreatedAt)
            };

            ViewBag.Positions = _context.Positions.OrderBy(p => p.PositionName).ToList();
            ViewBag.Teams = _context.Teams.OrderBy(t => t.TeamName).ToList();

            return View(employees.ToList());
        }
    }
}
