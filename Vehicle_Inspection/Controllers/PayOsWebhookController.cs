using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayOS;
using PayOS.Models;
using PayOS.Models.Webhooks;
using System;
using Vehicle_Inspection.Data;

[ApiController]
public class PayOsWebhookController : ControllerBase
{
    private readonly PayOSClient _payos;
    private readonly VehInsContext _db;

    public PayOsWebhookController(PayOSClient payos, VehInsContext db)
    {
        _payos = payos;
        _db = db;
    }

    [HttpPost("/payos/webhook")]
    public async Task<IActionResult> Webhook([FromBody] Webhook webhook)
    {
        // 1) Verify signature + parse data
        var data = await _payos.Webhooks.VerifyAsync(webhook); // đúng theo README :contentReference[oaicite:6]{index=6}

        // 2) Map orderCode -> PaymentId
        var paymentId = (int)data.OrderCode;

        var payment = await _db.Payments.SingleAsync(p => p.PaymentId == paymentId);

        // 3) Idempotent update
        // data.Data.Code == "00" (thành công) theo mẫu README :contentReference[oaicite:7]{index=7}
        if (data.Code == "00")
        {
            if (payment.PaymentStatus != 1)
            {
                payment.PaymentStatus = 1; // PAID
                payment.PaidAt = DateTime.UtcNow; // hoặc DateTime.Now / hoặc DB time
                // payment.PaidBy = ...
                await _db.SaveChangesAsync();

                // Nếu bạn muốn kéo Inspection.Status=PAID
                var ins = await _db.Inspections.SingleAsync(i => i.InspectionId == payment.InspectionId);
                if (ins.Status < 2) ins.Status = 2;
                await _db.SaveChangesAsync();
            }
        }
        else
        {
            Console.WriteLine("---------------------------[" + data.OrderCode + " đã hủy]---------------------------");
            // Có thể set CANCELLED tùy nghiệp vụ
        }

        return Ok();
    }
}
