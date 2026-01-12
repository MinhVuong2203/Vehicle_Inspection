using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
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

        private List<ProvinceDto> LoadProvinces()
        {
            var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Database", "SauXacNhap.json");
            var json = System.IO.File.ReadAllText(filePath);

            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<List<ProvinceDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<ProvinceDto>();
            }
            catch (System.Text.Json.JsonException ex)
            {
                // đặt breakpoint ở đây để xem ex.Path (nó sẽ chỉ ra field nào gây lỗi)
                throw;
            }
        }


        private void ParseAddressToFields(User employee)
        {
            if (string.IsNullOrWhiteSpace(employee.Address))
                return;

            var parts = employee.Address
                .Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToArray();

            if (parts.Length == 3)
            {
                employee.AddressLine = parts[0];
                employee.WardName = parts[1];
                employee.ProvinceName = parts[2];
                return;
            }

            // fallback nếu dữ liệu cũ
            employee.AddressLine = employee.Address;
        }


        //[GET]: Employee/Index
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
            if (employee == null) return NotFound();

            // Đảm bảo Account không null (vì bạn có bind Account.Username/PasswordHash)
            employee.Account ??= new Account();

            var provinces = LoadProvinces();
            ParseAddressToFields(employee);

            ViewBag.Provinces = provinces; // dùng để render tỉnh và JS
            ViewBag.Positions = _context.Positions.ToList();
            ViewBag.Teams = _context.Teams.ToList();

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, User employee)
        {
            if (id != employee.UserId) return BadRequest();

            // 1) Validate 3 field địa chỉ tách
            if (string.IsNullOrWhiteSpace(employee.AddressLine))
                ModelState.AddModelError(nameof(employee.AddressLine), "Vui lòng nhập số nhà / đường");

            if (string.IsNullOrWhiteSpace(employee.WardName))
                ModelState.AddModelError(nameof(employee.WardName), "Vui lòng chọn phường / xã");

            if (string.IsNullOrWhiteSpace(employee.ProvinceName))
                ModelState.AddModelError(nameof(employee.ProvinceName), "Vui lòng chọn tỉnh / thành phố");

            // 2) GHÉP Address TRƯỚC khi kiểm tra ModelState
            if (!string.IsNullOrWhiteSpace(employee.AddressLine)
                && !string.IsNullOrWhiteSpace(employee.WardName)
                && !string.IsNullOrWhiteSpace(employee.ProvinceName))
            {
                employee.Address = $"{employee.AddressLine.Trim()} | {employee.WardName.Trim()} | {employee.ProvinceName.Trim()}";
                ModelState.Remove("Address");
            }

            // 3) Bỏ validate navigation properties
            ModelState.Remove("Account.User");
            ModelState.Remove("Position");
            ModelState.Remove("Team");
            ModelState.Remove("Certificates");
            ModelState.Remove("InspectionConcludedByNavigations");
            ModelState.Remove("InspectionCreatedByNavigations");
            ModelState.Remove("InspectionDefects");
            ModelState.Remove("InspectionReceivedByNavigations");
            ModelState.Remove("InspectionStages");
            ModelState.Remove("PasswordRecoveries");
            ModelState.Remove("PaymentCreatedByNavigations");
            ModelState.Remove("PaymentPaidByNavigations");
            ModelState.Remove("SpecificationCreatedByNavigations");
            ModelState.Remove("SpecificationUpdatedByNavigations");
            ModelState.Remove("VehicleCreatedByNavigations");
            ModelState.Remove("VehicleUpdatedByNavigations");
            ModelState.Remove("Roles");
            ModelState.Remove("Stages");

            if (!ModelState.IsValid)
            {
                ViewBag.Provinces = LoadProvinces();
                ViewBag.Positions = _context.Positions.ToList();
                ViewBag.Teams = _context.Teams.ToList();
                return View(employee);
            }

            try
            {
                await _employeeService.UpdateEmployeeAsync(employee);
                TempData["SuccessMessage"] = "Cập nhật nhân viên thành công!";
                // Debug: Kiểm tra TempData có được set không
                Console.WriteLine($"TempData set: {TempData["SuccessMessage"]}");
                return RedirectToAction(nameof(Index), new { showSuccess = "true" });
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                // Debug: Kiểm tra TempData có được set không
                Console.WriteLine($"TempData set: {TempData["ErrorMessage"]}");
                ViewBag.Provinces = LoadProvinces();
                ViewBag.Positions = _context.Positions.ToList();
                ViewBag.Teams = _context.Teams.ToList();
                return View(employee);
            }
        }


    }
}