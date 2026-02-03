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
        public IActionResult CollectPayment(int paymentId, string paymentMethod, string? note)
        {
            try
            {
                var userId = GetCurrentUserId();

                if (paymentMethod == "Chuyển khoản")
                {
                    TempData["InfoMessage"] = "Thanh toán PayOS sẽ xử lý ở form riêng";
                    return RedirectToAction(nameof(Index));
                }

                if (userId == Guid.Empty)
                {
                    TempData["ErrorMessage"] = "Không xác định được người dùng.";
                    return RedirectToAction(nameof(Index));
                }

                var result = _tollService.CollectPayment(paymentId, paymentMethod, note, userId);

                switch (result)
                {
                    case "Success":
                        TempData["SuccessMessage"] = "Thu phí thành công!";
                        break;

                    case "Not found":
                        TempData["ErrorMessage"] = "Không tìm thấy payment.";
                        break;

                    case "Successed":
                        TempData["ErrorMessage"] = "Payment này đã thanh toán.";
                        break;

                    case "Failed":
                        TempData["ErrorMessage"] = "Payment đã bị hủy.";
                        break;

                    default:
                        TempData["ErrorMessage"] = "Có lỗi xảy ra.";
                        break;
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
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
            var payment = _tollService.GetPaymentByOrderCode(orderCode);

            if (payment == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy payment.";
                return RedirectToAction(nameof(Index));
            }

            if (payment.PaymentStatus == 1)
            {
                TempData["InfoMessage"] = "Payment đã thanh toán.";
                return RedirectToAction(nameof(Index));
            }

            if (payment.PaymentStatus == 2)
            {
                TempData["InfoMessage"] = "Payment đã bị hủy.";
                return RedirectToAction(nameof(Index));
            }

            if (!cancel && string.Equals(status, "PAID", StringComparison.OrdinalIgnoreCase))
            {
                try
                {
                    payment.PaymentMethod = "Chuyển khoản";
                    payment.PaymentStatus = 1;
                    payment.PaidAt = DateTime.Now;
                    payment.ReceiptPrintCount++;

                    payment.Inspection.PaidAt = DateTime.Now;

                    if (payment.Inspection.Status == 1)
                        payment.Inspection.Status = 2;

                    _context.SaveChanges();

                    TempData["SuccessMessage"] = $"Thanh toán thành công {payment.Inspection.InspectionCode}";
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = ex.Message;
                }
            }
            else
            {
                TempData["ErrorMessage"] = "Thanh toán chưa hoàn tất.";
            }

            return RedirectToAction(nameof(Index));
        }


        [HttpGet("/toll/cancel")]
        public IActionResult Cancel(long? orderCode)
        {
            var payment = _tollService.GetPaymentByOrderCode(orderCode);

            if (payment != null)
            {
                payment.PaymentStatus = 2;
                _context.SaveChanges();

                TempData["InfoMessage"] = $"Bạn đã hủy thanh toán {payment.Inspection.InspectionCode}";
            }

            return RedirectToAction(nameof(Index));
        }



    }
}