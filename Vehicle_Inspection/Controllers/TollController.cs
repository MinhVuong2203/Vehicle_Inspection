using Microsoft.AspNetCore.Mvc;
using Vehicle_Inspection.Service;
using Vehicle_Inspection.Models;
using System.Security.Claims;

namespace Vehicle_Inspection.Controllers
{
    public class TollController : Controller
    {
        private readonly ITollService _tollService;

        public TollController(ITollService tollService)
        {
            _tollService = tollService;
        }

        // GET: Toll/Index
        public IActionResult Index(string? search, short? status, string? type)
        {
            // Mặc định lấy các đơn chờ thu phí (status = 1) hoặc đã hoàn thành kiểm định
            if (!status.HasValue && string.IsNullOrEmpty(search) && string.IsNullOrEmpty(type))
            {
                status = 1; // 1 = Chờ thu phí/tiếp nhận
            }

            var inspections = _tollService.GetInspections(search, status);

            // Lọc theo loại kiểm định nếu có
            if (!string.IsNullOrEmpty(type))
            {
                inspections = inspections.Where(i => i.InspectionType == type).ToList();
            }

            // Truyền dữ liệu tìm kiếm vào ViewBag để giữ giá trị trong form
            ViewBag.SearchValue = search;
            ViewBag.StatusValue = status;
            ViewBag.TypeValue = type;

            // Tính toán thống kê
            // Status = 1: Chờ thu phí
            // Status = 2 trở lên: Đã thu phí hoặc các trạng thái khác
            ViewBag.PendingCount = inspections.Count(i => i.Status == 1 && i.PaidAt == null);
            ViewBag.CompletedCount = inspections.Count(i => i.PaidAt != null);
            ViewBag.TotalCount = inspections.Count;

            return View(inspections);
        }

        // POST: Toll/CollectPayment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CollectPayment(string inspectionCode, string paymentMethod, string? note)
        {
            try
            {
                // Lấy userId từ session/claims (tùy vào cách bạn quản lý authentication)
                var userId = GetCurrentUserId();

                if (userId == Guid.Empty)
                {
                    TempData["ErrorMessage"] = "Không xác định được người dùng. Vui lòng đăng nhập lại.";
                    return RedirectToAction(nameof(Index));
                }

                var result = _tollService.CollectPayment(inspectionCode, paymentMethod, note, userId);

                if (result)
                {
                    TempData["SuccessMessage"] = $"Thu phí thành công cho đơn kiểm định {inspectionCode}!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Thu phí thất bại. Đơn kiểm định có thể đã được thu phí hoặc không tồn tại.";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Có lỗi xảy ra: {ex.Message}";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Toll/Details
        public IActionResult Details(string inspectionCode)
        {
            var inspection = _tollService.GetInspectionDetails(inspectionCode);

            if (inspection == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn kiểm định.";
                return RedirectToAction(nameof(Index));
            }

            return View(inspection);
        }

        // Helper method để lấy userId hiện tại
        private Guid GetCurrentUserId()
        {
            // Cách 1: Nếu dùng Claims-based authentication
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userIdClaim) && Guid.TryParse(userIdClaim, out Guid userId))
            {
                return userId;
            }

            // Cách 2: Nếu dùng Session
            // var userIdSession = HttpContext.Session.GetString("UserId");
            // if (!string.IsNullOrEmpty(userIdSession) && Guid.TryParse(userIdSession, out Guid userId))
            // {
            //     return userId;
            // }

            // Cách 3: Mặc định cho development (XÓA KHI DEPLOY)
            // return Guid.Parse("00000000-0000-0000-0000-000000000001");

            return Guid.Empty;
        }
    }
}