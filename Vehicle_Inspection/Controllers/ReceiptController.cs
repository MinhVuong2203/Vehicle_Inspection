using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vehicle_Inspection.Data;

namespace Vehicle_Inspection.Controllers
{
    public class ReceiptController : Controller
    {
        private readonly VehInsContext _db;
        public ReceiptController(VehInsContext db) => _db = db;

        // GET: /receipt/printbyinspectioncode?inspectionCode=...
        [HttpGet("/receipt/printbyinspectioncode")]
        public async Task<IActionResult> PrintByInspectionCode(string inspectionCode)
        {
            if (string.IsNullOrWhiteSpace(inspectionCode))
                return BadRequest("Thiếu inspectionCode");

            var inspection = await _db.Inspections
                .AsNoTracking()
                .Include(i => i.Vehicle)
                //.Include(i => i.Owner)
                .Include(i => i.Payment)
                .FirstOrDefaultAsync(i => i.InspectionCode == inspectionCode && !i.IsDeleted);

            if (inspection == null) return NotFound("Không tìm thấy đơn kiểm định");

            // chỉ cho in khi đã thanh toán
            if (inspection.Payment == null || inspection.Payment.PaymentStatus != 1)
                return BadRequest("Đơn chưa thanh toán, không thể in biên nhận.");

            return View("Print", inspection); // Views/Receipt/Print.cshtml
        }
    }
}
