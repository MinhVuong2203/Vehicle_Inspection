using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Vehicle_Inspection.Data;
using Vehicle_Inspection.Models;
using Vehicle_Inspection.Service;

namespace Vehicle_Inspection.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly VehInsContext _context;
        private readonly IEmployee _employeeService;

        public EmployeeController(VehInsContext context, IEmployee employeeService)
        {
            _context = context;
            _employeeService = employeeService;
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

        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(User employee)
        {
            if (ModelState.IsValid)
            {
                await _employeeService.CreateEmployeeAsync(employee);
                return RedirectToAction("Index");
            }
            return View(employee);
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);
            if (employee == null)
            {
                return NotFound();
            }

            // Load provinces from SauXacNhap.json
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Database", "SauXacNhap.json");
            var jsonData = System.IO.File.ReadAllText(filePath);
            var provinces = JsonConvert.DeserializeObject<List<dynamic>>(jsonData);

            ViewBag.Provinces = provinces;
            ViewBag.Positions = _context.Positions.ToList();
            ViewBag.Teams = _context.Teams.ToList();

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, User employee)
        {
            if (id != employee.UserId)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                await _employeeService.UpdateEmployeeAsync(employee);
                return RedirectToAction("Index");
            }
            ViewBag.Positions = _context.Positions.ToList();
            ViewBag.Teams = _context.Teams.ToList();
            return View(employee);
        }
    }
}