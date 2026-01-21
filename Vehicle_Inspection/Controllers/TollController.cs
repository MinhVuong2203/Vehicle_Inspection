using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Vehicle_Inspection.Data;
using Vehicle_Inspection.Models;
using Vehicle_Inspection.Service;

namespace Vehicle_Inspection.Controllers
{
    public class TollController : Controller
    {
        private readonly ITollService _tollService;
        private readonly VehInsContext _context;

        public TollController(ITollService tollService, VehInsContext context)
        {
            _tollService = tollService;
            _context = context;
        }

        // GET: Toll/Index
        public IActionResult Index(string? search, short? status, string? type)
        {
            // Mặc định lấy các đơn chờ thu phí (status = 1) hoặc đã hoàn thành kiểm định
            //if (!status.HasValue && string.IsNullOrEmpty(search) && string.IsNullOrEmpty(type))
            //{
            //    status = 1; // 1 = Chờ thu phí/tiếp nhận
            //}

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
        // Form này submit cho thanh toán tiền mặt
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CollectPayment(string inspectionCode, string paymentMethod, string? note)
        {
            try
            {
                // Lấy userId từ session/claims (tùy vào cách bạn quản lý authentication)
                var userId = GetCurrentUserId();
 

                if (paymentMethod == "Chuyển khoản")
                {
                    TempData["InfoMessage"] = "Thanh toán bằng PayOS có gọi form rồi";
                    return RedirectToAction(nameof(Index));
                }


                if (userId == Guid.Empty)
                {
                    TempData["ErrorMessage"] = "Không xác định được người dùng. Vui lòng đăng nhập lại.";
                    return RedirectToAction(nameof(Index));
                }

                string result = _tollService.CollectPayment(inspectionCode, paymentMethod, note, userId); // Thành công Đơn tiền mặt

                if (result == "Success")
                {
                    TempData["SuccessMessage"] = $"Thu phí thành công cho đơn kiểm định {inspectionCode}!";
                }
                else if (result == "Not found")
                {
                    TempData["ErrorMessage"] = "Thu phí thất bại. Đơn kiểm định này không tồn tại!";
                }
                else if (result == "Successed")
                {
                    TempData["ErrorMessage"] = "Đơn kiểm định này đã được thu phí.";
                }
                else if (result == "Failed")
                {
                    TempData["ErrorMessage"] = "Đơn kiểm định này đã bị hủy";
                }
                else {
                    TempData["ErrorMessage"] = "Có gì đó sai sai!";
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

        // TRẢ VỀ KHI THANH TOÁN THÀNH CÔNG
        [HttpGet("/toll/return")]
        public IActionResult Return(string? code, string? id, bool cancel, string? status, long? orderCode)
        {

            Inspection inspection = _tollService.getInspectionByOrderCode(orderCode);

            if (inspection == null)
            {
                TempData["ErrorMessage"] = $"Không tìm thấy khách hàng với mã thanh toán.";
                return RedirectToAction(nameof(Index));
            } 
            else if (inspection.Payment != null && inspection.Payment.PaymentStatus == 1)
            {
                TempData["InfoMessage"] = $"Khách hàng này đã thanh toán";
                return RedirectToAction(nameof(Index));
            }
            else if (inspection.Payment != null && inspection.Payment.PaymentStatus == 2)
            {
                // Cần xử lý xét PaymentStatus = 0 để thanh toán lại
                TempData["InfoMessage"] = $"Giao dịch này đã bị hủy";
                return RedirectToAction(nameof(Index));
            }
            
            // PayOS trả về PAID ở status
            if (!cancel && string.Equals(status, "PAID", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    inspection.Payment.PaymentMethod = "Chuyển khoản";
                    inspection.Payment.PaymentStatus = 1;
                    inspection.Payment.ReceiptPrintCount++;
                    inspection.Payment.PaidAt = DateTime.Now;
                    inspection.Status = 2;
                    inspection.PaidAt = DateTime.Now;
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = $"Thanh toán thành công {inspection.InspectionCode}.";               
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = $"Có lỗi xảy ra khi cập nhật thanh toán: {ex.Message}";
                }
            }
            else
            { 
                TempData["ErrorMessage"] = $"Thanh toán chưa hoàn tất (status={status}, orderCode={inspection.InspectionCode}).";
            }

            return RedirectToAction(nameof(Index));
        }

        // TRẢ VỀ KHI HỦY THANH TOÁN
        [HttpGet("/toll/cancel")]
        public IActionResult Cancel(string? code, string? id, bool cancel, string? status, long? orderCode)
        {
            Inspection i = _tollService.getInspectionByOrderCode(orderCode);
            TempData["InfoMessage"] = $"Bạn đã hủy thanh toán {i.InspectionCode}";
            return RedirectToAction(nameof(Index));
        }



    }
}