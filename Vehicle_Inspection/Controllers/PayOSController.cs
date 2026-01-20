using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayOS;
using PayOS.Models.V2.PaymentRequests;
using System;
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

        [HttpPost("create-link/{inspectionId:int}")]
        public async Task<IActionResult> CreateLink(int inspectionId)
        {
            var payment = await _db.Payments
                .AsNoTracking()
                .SingleAsync(p => p.InspectionId == inspectionId);

            // orderCode: payOS yêu cầu số (long/int). lấy khoảng cách từ 1/1/1970 đến hiện tại tính bằng milliseconds
            long orderCode = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

            // Lưu xuống DB để map khi return gọi về
            payment.OrderCode = orderCode;
            await _db.SaveChangesAsync();

            var req = new CreatePaymentLinkRequest
            {
                OrderCode = orderCode,
                Amount = (long)payment.TotalAmount,              // VND nguyên
                Description = $"Thanh toán {inspectionId}",
                ReturnUrl = _cfg["PayOS:ReturnUrl"]!,
                CancelUrl = _cfg["PayOS:CancelUrl"]!
            };

            var link = await _payos.PaymentRequests.CreateAsync(req); // đúng theo README :contentReference[oaicite:4]{index=4}

            // Bạn nên lưu link.PaymentLinkId / link.CheckoutUrl vào DB (nếu model có)
            // rồi return checkoutUrl để FE redirect
            return Ok(new { checkoutUrl = link.CheckoutUrl, orderCode });
        }


       
    }
}
