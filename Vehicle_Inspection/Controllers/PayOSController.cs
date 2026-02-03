using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using System.Security.Claims;
using Vehicle_Inspection.Data;

namespace Vehicle_Inspection.Controllers
{
    [ApiController]
    [Route("api/payos")]
    public class PayOSController : ControllerBase
    {
        private readonly PayOSClient _payos;
        private readonly VehInsContext _db;
        private readonly IConfiguration _cfg;

        public PayOSController(PayOSClient payos, VehInsContext db, IConfiguration cfg)
        {
            _payos = payos;
            _db = db;
            _cfg = cfg;
        }

        /// <summary>
        /// Tạo link thanh toán PayOS cho một Payment cụ thể
        /// </summary>
        [HttpPost("create-link/{paymentId:int}")]
        public async Task<IActionResult> CreateLink(int paymentId)
        {
            // 1) Lấy userId người đang đăng nhập
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
            {
                return Unauthorized("Không xác định được user đăng nhập.");
            }

            // 2) Tìm Payment theo paymentId (kèm Inspection để lấy InspectionCode)
            var payment = await _db.Payments
                .Include(p => p.Inspection)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
            {
                return NotFound($"Không tìm thấy payment với ID: {paymentId}");
            }

            // 3) Kiểm tra trạng thái payment
            if (payment.PaymentStatus == 1)
            {
                return BadRequest("Payment này đã được thanh toán.");
            }

            if (payment.PaymentStatus == 2)
            {
                return BadRequest("Payment này đã bị hủy.");
            }

            // 4) Tạo OrderCode (số duy nhất cho PayOS)
            long orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // 5) Cập nhật thông tin vào Payment để map khi return
            payment.OrderCode = orderCode;
            payment.PaidBy = userId;
            payment.Notes = $"CK-{payment.Inspection.InspectionCode}";

            Console.WriteLine($"------------- OrderCode: {orderCode} | PaymentId: {paymentId} --------------------");

            await _db.SaveChangesAsync();

            // 6) Tạo request cho PayOS
            var req = new CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = (long)payment.TotalAmount,  // VND nguyên
                Description = $"CK-{payment.Inspection.InspectionCode}",
                ReturnUrl = _cfg["PayOS:ReturnUrl"]!,
                CancelUrl = _cfg["PayOS:CancelUrl"]!
            };

            // 7) Gọi PayOS API để tạo link
            var link = await _payos.PaymentRequests.CreateAsync(req);

            // 8) (Optional) Lưu PaymentLinkId nếu cần
            // payment.PaymentLinkId = link.PaymentLinkId;
            // await _db.SaveChangesAsync();

            // 9) Trả về checkoutUrl để frontend redirect
            return Ok(new 
            { 
                checkoutUrl = link.CheckoutUrl, 
                orderCode = orderCode,
                paymentId = paymentId
            });
        }
    }
}