using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
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

        #region Helper Methods

        private List<ProvinceDto> LoadProvinces()
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Database", "SauXacNhap.json");

                if (!System.IO.File.Exists(filePath))
                {
                    return new List<ProvinceDto>();
                }

                var json = System.IO.File.ReadAllText(filePath);

                return JsonSerializer.Deserialize<List<ProvinceDto>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? new List<ProvinceDto>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading provinces: {ex.Message}");
                return new List<ProvinceDto>();
            }
        }


        private void LoadViewBagData()
        {
            ViewBag.Provinces = LoadProvinces();
            ViewBag.Positions = _context.Positions.OrderBy(p => p.PositionName).ToList();
            ViewBag.Teams = _context.Teams.OrderBy(t => t.TeamName).ToList();
        }

        #endregion

        #region Actions

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

        public async Task<IActionResult> Create()
        {
            LoadViewBagData();

            // để bind Account.Username/PasswordHash không bị null reference
            var model = new User
            {
                IsActive = true,
                CreatedAt = DateTime.Now,
                Account = new Account()
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("FullName,Phone,Email,BirthDate,CCCD,AddressLine,WardName,ProvinceName,Gender,PositionId,TeamId,Level,Account")]
            User employee)
        {
            if (!ModelState.IsValid)
            {
                LoadViewBagData();
                return View(employee);
            }

            try
            {
                await _employeeService.CreateEmployeeAsync(employee);
                TempData["SuccessMessage"] = "Thêm nhân viên thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                LoadViewBagData();
                return View(employee);
            }
        }

        public async Task<IActionResult> Edit(Guid id)
        {
            var employee = await _employeeService.GetEmployeeByIdAsync(id);

            if (employee == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy nhân viên!";
                return RedirectToAction(nameof(Index));
            }

            // Đảm bảo Account không null
            employee.Account ??= new Account();

            LoadViewBagData();

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Guid id,
            [Bind("UserId,FullName,Phone,Email,BirthDate,CCCD,Gender,Address,Ward,Province,PositionId,TeamId,Level,IsActive,CreatedAt,ImageUrl,Account")]
            User employee)
        {
            if (id != employee.UserId)
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ!";
                return RedirectToAction(nameof(Index));
            }

            //// Validate các field địa chỉ
            //if (string.IsNullOrWhiteSpace(employee.AddressLine))
            //{
            //    ModelState.AddModelError(nameof(employee.AddressLine), "Vui lòng nhập số nhà / đường");
            //}

            //if (string.IsNullOrWhiteSpace(employee.WardName))
            //{
            //    ModelState.AddModelError(nameof(employee.WardName), "Vui lòng chọn phường / xã");
            //}

            //if (string.IsNullOrWhiteSpace(employee.ProvinceName))
            //{
            //    ModelState.AddModelError(nameof(employee.ProvinceName), "Vui lòng chọn tỉnh / thành phố");
            //}

            // Ghép địa chỉ

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Form không hợp lệ.";
                LoadViewBagData();
                return View(employee);
            }

            try
            {
                await _employeeService.UpdateEmployeeAsync(employee);
                TempData["SuccessMessage"] = "Cập nhật " + employee.FullName + " thành công!";
                return LocalRedirect(Url.Action("Index", "Employee")!);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await EmployeeExists(id))
                {
                    TempData["ErrorMessage"] = "Nhân viên không tồn tại!";
                    return RedirectToAction(nameof(Index));
                }
                throw;
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                LoadViewBagData();
                return View(employee);
            }
        }

        private async Task<bool> EmployeeExists(Guid id)
        {
            return await _context.Users.AnyAsync(e => e.UserId == id);
        }

        #endregion
    }
}