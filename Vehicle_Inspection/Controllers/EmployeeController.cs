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

        [HttpGet]
        public async Task<IActionResult> GetTeamsByPosition(int positionId)
        {
            var teams = await _context.Positions
                .Where(p => p.PositionId == positionId)
                .SelectMany(p => p.Teams)
                .Select(t => new
                {
                    teamId = t.TeamId,
                    teamName = t.TeamName
                })
                .OrderBy(t => t.teamName)
                .ToListAsync();

            return Json(teams);
        }




        public IActionResult Index(string? search, int? position, int? team, string? gender, bool? isActive, string? sort, int page = 1)
        {
            int pageSize = 11;

            var employees = _context.Users
          
                .Include(u => u.Position)
                .Include(u => u.Team)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim().ToLower();
                employees = employees.Where(u =>
                    u.FullName.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search) ||
                    u.Phone.Contains(search));
            }

            if (position.HasValue)
                employees = employees.Where(u => u.PositionId == position.Value);

            if (team.HasValue)
                employees = employees.Where(u => u.TeamId == team.Value);

            if (!string.IsNullOrWhiteSpace(gender))
                employees = employees.Where(u => u.Gender == gender);

            if (isActive.HasValue)
                employees = employees.Where(u => u.IsActive == isActive.Value);

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

            // Đếm tổng số bản ghi sau khi filter
            var totalItems = employees.Count();

            // Phân trang
            var paginatedEmployees = employees
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            // Truyền thông tin phân trang qua ViewBag
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.TotalItems = totalItems;

            ViewBag.Positions = _context.Positions.OrderBy(p => p.PositionName).ToList();
            ViewBag.Teams = _context.Teams.OrderBy(t => t.TeamName).ToList();

            return View(paginatedEmployees);
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
      [Bind("FullName,Phone,Email,BirthDate,CCCD,Address,Ward,Province,Gender,PositionId,TeamId,Level,Account")] User employee, IFormFile? ProfilePicture)
        {
            // Set created fields
            employee.UserId = Guid.NewGuid();
            employee.CreatedAt = DateTime.Now;
            employee.IsActive = true;


            if (!string.IsNullOrWhiteSpace(employee.Email))
            {
                var existsEmail = await _context.Users.AnyAsync(x => x.Email == employee.Email);
                if (existsEmail) ModelState.AddModelError(nameof(employee.Email), "Email đã tồn tại.");
            }

            if (!string.IsNullOrWhiteSpace(employee.Phone))
            {
                var existsPhone = await _context.Users.AnyAsync(x => x.Phone == employee.Phone);
                if (existsPhone) ModelState.AddModelError(nameof(employee.Phone), "Số điện thoại đã tồn tại.");
            }

            if (!string.IsNullOrWhiteSpace(employee.CCCD))
            {
                var existsCccd = await _context.Users.AnyAsync(x => x.CCCD == employee.CCCD);
                if (existsCccd) ModelState.AddModelError(nameof(employee.CCCD), "CCCD đã tồn tại.");
            }


            // (Tuỳ chọn) Check trùng Username nếu DB có unique cho Username
            if (!string.IsNullOrWhiteSpace(employee.Account?.Username))
            {
                var existsUsername = await _context.Accounts.AnyAsync(a => a.Username == employee.Account.Username);
                if (existsUsername) ModelState.AddModelError("Account.Username", "Tên đăng nhập đã tồn tại.");
            }

            // Upload ảnh (nếu có) - bạn thay path theo project của bạn
            if (ProfilePicture != null && ProfilePicture.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "employee");
                if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);

                var fileName = $"{employee.UserId}{Path.GetExtension(ProfilePicture.FileName)}";
                var filePath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfilePicture.CopyToAsync(stream);
                }

                employee.ImageUrl = $"/images/employee/{fileName}";
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ. Vui lòng kiểm tra lại.";
                LoadViewBagData();
                employee.Account ??= new Account();
                return View(employee);
            }

            try
            {
                // Ensure account exists
                employee.Account ??= new Account();
                employee.Account.UserId = employee.UserId;

                // Hash password nếu có nhập
                if (!string.IsNullOrWhiteSpace(employee.Account.PasswordHash))
                {
                    employee.Account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(employee.Account.PasswordHash);
                }

                await _employeeService.CreateEmployeeAsync(employee);

                TempData["SuccessMessage"] = "Thêm nhân viên thành công!";
                return RedirectToAction("Index", "Employee");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                LoadViewBagData();
                employee.Account ??= new Account();
                return View(employee);
            }
        }

        [HttpGet]
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

            // Truyền thông tin Giám đốc hiện tại để kiểm tra trong View
            ViewBag.IsCurrentDirector = await IsDirector(employee.PositionId);

            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(
            Guid id,
            [Bind("UserId,FullName,Phone,Email,BirthDate,CCCD,Gender,Address,Ward,Province,PositionId,TeamId,Level,IsActive,CreatedAt,ImageUrl,Account")]
    User employee,
            IFormFile? ProfilePicture)
        {
            if (id != employee.UserId)
            {
                TempData["ErrorMessage"] = "Dữ liệu không hợp lệ!";
                return RedirectToAction(nameof(Index));
            }

            // =====================================================
            // KIỂM TRA LOGIC GIÁM ĐỐC
            // =====================================================
            var currentEmployee = await _employeeService.GetEmployeeByIdAsync(id);
            if (currentEmployee == null)
            {
                TempData["ErrorMessage"] = "Nhân viên không tồn tại!";
                return RedirectToAction(nameof(Index));
            }

            // Lấy thông tin position hiện tại và position mới
            var currentPosition = await _context.Positions
                .FirstOrDefaultAsync(p => p.PositionId == currentEmployee.PositionId);

            var newPosition = await _context.Positions
                .FirstOrDefaultAsync(p => p.PositionId == employee.PositionId);

            // CASE 1: Nhân viên hiện tại là Giám đốc và muốn đổi sang vị trí khác
            if (currentPosition?.PoitionCode == "GD" && newPosition?.PoitionCode != "GD")
            {
                // Đếm số lượng Giám đốc hiện có trong hệ thống
                var directorCount = await _context.Users
                    .Include(u => u.Position)
                    .Where(u =>
                        u.Position!.PoitionCode == "GD" &&
                        u.IsActive) // Nếu có soft delete
                    .CountAsync();

                // Chỉ chặn nếu là Giám đốc duy nhất
                if (directorCount == 1)
                {
                    TempData["ErrorMessage"] = "Không thể thay đổi chức vụ Giám đốc! Hệ thống phải có ít nhất 1 Giám đốc. Hiện tại bạn là Giám đốc duy nhất.";
                    LoadViewBagData();
                    ViewBag.IsCurrentDirector = true;
                    return View(employee);
                }
            }

            // CASE 2: Nhân viên khác muốn lên làm Giám đốc
            if (currentPosition?.PoitionCode != "GD" && newPosition?.PoitionCode == "GD")
            {
                // Kiểm tra xem đã có Giám đốc khác chưa
                var existingDirector = await _context.Users
                    .Include(u => u.Position)
                    .FirstOrDefaultAsync(u =>
                        u.Position!.PoitionCode == "GD" &&
                        u.UserId != id &&
                        u.IsActive);

                if (existingDirector != null)
                {
                    TempData["ErrorMessage"] = $"Đã có Giám đốc trong hệ thống ({existingDirector.FullName}).";
                    LoadViewBagData();
                    ViewBag.IsCurrentDirector = false;
                    return View(employee);
                }
            }

            // =====================================================
            // UPLOAD ẢNH (nếu có)
            // =====================================================
            if (ProfilePicture != null && ProfilePicture.Length > 0)
            {
                var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "employee");
                if (!Directory.Exists(uploads))
                    Directory.CreateDirectory(uploads);

                var fileName = $"{employee.UserId}{Path.GetExtension(ProfilePicture.FileName)}";
                var filePath = Path.Combine(uploads, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await ProfilePicture.CopyToAsync(stream);
                }

                employee.ImageUrl = $"/images/employee/{fileName}";
            }
            else
            {
                // Giữ nguyên ImageUrl cũ nếu không upload ảnh mới
                employee.ImageUrl = currentEmployee.ImageUrl;
            }

            // =====================================================
            // VALIDATION
            // =====================================================
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Form không hợp lệ.";
                LoadViewBagData();
                ViewBag.IsCurrentDirector = await IsDirector(employee.PositionId);
                return View(employee);
            }

            // =====================================================
            // CẬP NHẬT
            // =====================================================
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
                ViewBag.IsCurrentDirector = await IsDirector(employee.PositionId);
                return View(employee);
            }
        }

        // =====================================================
        // HELPER METHODS
        // =====================================================

        /// <summary>
        /// Kiểm tra xem PositionId có phải là Giám đốc không
        /// </summary>
        private async Task<bool> IsDirector(int? positionId)
        {
            if (positionId == null) return false;

            var position = await _context.Positions
                .FirstOrDefaultAsync(p => p.PositionId == positionId);

            return position?.PoitionCode == "GD";
        }


        private async Task<bool> EmployeeExists(Guid id)
        {
            return await _context.Users.AnyAsync(e => e.UserId == id);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteSoft(Guid id)
        {
            try
            {
                await _employeeService.DeleteSoftAsync(id);
                TempData["SuccessMessage"] = "Xoá cán bộ thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore(Guid id)
        {
            try
            {
                await _employeeService.RestoreAsync(id);
                TempData["SuccessMessage"] = "Khôi phục cán bộ thành công!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
                return RedirectToAction(nameof(Index));
            }
        }

        #endregion
    }
}