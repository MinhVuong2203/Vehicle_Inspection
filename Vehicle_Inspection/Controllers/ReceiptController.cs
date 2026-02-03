using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vehicle_Inspection.Data;

namespace Vehicle_Inspection.Controllers
{
    public class ReceiptController : Controller
    {
        private readonly VehInsContext _db;

        public ReceiptController(VehInsContext db) => _db = db;

        // GET: /receipt/printbypaymentid?paymentId=...
        [HttpGet("/receipt/printbypaymentid")]
        public async Task<IActionResult> PrintByPaymentId(int paymentId)
        {
            if (paymentId <= 0)
                return BadRequest("PaymentId không hợp lệ");

            var payment = await _db.Payments
                .AsNoTracking()
                .Include(p => p.Inspection)
                    .ThenInclude(i => i.Vehicle)
                        .ThenInclude(v => v.Owner)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
                return NotFound("Không tìm thấy payment");

            // Chỉ cho in khi đã thanh toán
            if (payment.PaymentStatus != 1)
                return BadRequest("Payment chưa thanh toán, không thể in biên nhận.");

            return View("Print", payment); // Views/Receipt/Print.cshtml
        }
    }
}