using Microsoft.EntityFrameworkCore;
using Vehicle_Inspection.Data;
using Vehicle_Inspection.Models;

namespace Vehicle_Inspection.Service
{
    public class TollService : ITollService
    {
        private readonly VehInsContext _context;

        public TollService(VehInsContext context)
        {
            _context = context;
        }



        public List<Payment> GetPayments(string? search, short? status)
        {
            var query = _context.Payments
                .Include(p => p.Inspection)
                    .ThenInclude(i => i.Vehicle)
                        .ThenInclude(v => v.Owner)
                .Where(p => !p.Inspection.IsDeleted);

            if (status.HasValue)
                query = query.Where(p => p.PaymentStatus == status.Value);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => p.Inspection.InspectionCode.Contains(search));

            return query.OrderByDescending(p => p.CreatedAt).ToList();
        }


        public Inspection? GetInspectionDetails(string inspectionCode)
        {
            return _context.Inspections
                .Include(i => i.Vehicle)
                 .ThenInclude(v => v.Owner)
                .Include(i => i.Payments)
                .FirstOrDefault(i => i.InspectionCode == inspectionCode && !i.IsDeleted);
        }

        public Payment? GetPaymentByOrderCode(long? orderCode)
        {
            return _context.Payments
                .Include(p => p.Inspection)
                .FirstOrDefault(p => p.OrderCode == orderCode);
        }


        // Phục vụ cho thanh toán tiền mặt
        public string CollectPayment(int paymentId, string paymentMethod, string? note, Guid userId)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var payment = _context.Payments
                    .Include(p => p.Inspection)
                    .FirstOrDefault(p => p.PaymentId == paymentId);

                if (payment == null)
                    return "Not found";

                if (payment.PaymentStatus == 1)
                    return "Successed";

                if (payment.PaymentStatus == 2)
                    return "Failed";

                payment.PaymentMethod = paymentMethod;
                payment.PaymentStatus = 1;
                payment.PaidAt = DateTime.Now;
                payment.PaidBy = userId;
                payment.Notes = note;
                payment.ReceiptPrintCount = (payment.ReceiptPrintCount ?? 0) + 1;

                // update inspection
                var inspection = payment.Inspection;
                inspection.PaidAt = DateTime.Now;

                if (inspection.Status == 1)
                    inspection.Status = 2;

                _context.SaveChanges();
                transaction.Commit();

                return "Success";
            }
            catch
            {
                transaction.Rollback();
                return "Error";
            }
        }


        public async Task<(bool Success, string Message, Payment? NewPayment)> CreateAdditionalPaymentAsync(int inspectionId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Kiểm tra inspection có tồn tại không
                var inspection = await _context.Inspections
                    .Include(i => i.Payments)
                    .FirstOrDefaultAsync(i => i.InspectionId == inspectionId && !i.IsDeleted);

                if (inspection == null)
                {
                    return (false, "Không tìm thấy đơn kiểm định.", null);
                }

                // 2. Lấy payment gốc (payment đầu tiên hoặc payment gần nhất)
                var originalPayment = inspection.Payments
                    .OrderBy(p => p.CreatedAt)
                    .FirstOrDefault();

                if (originalPayment == null)
                {
                    return (false, "Đơn kiểm định chưa có payment nào.", null);
                }

                // 3. Tính phí dựa trên ngày tạo
                decimal feePercentage = CalculateFeePercentage(originalPayment.CreatedAt);

                decimal baseFee = originalPayment.BaseFee * feePercentage;
                decimal certificateFee = (originalPayment.CertificateFee ?? 0) * feePercentage;
                decimal stickerFee = (originalPayment.StickerFee ?? 0) * feePercentage;
                decimal totalAmount = baseFee + certificateFee + stickerFee;

                // 3. Tạo ReceiptNo mới
                string datePrefix = DateTime.Now.ToString("yyyyMMddHHmmssfff");
                string receiptNo = "RC-" + datePrefix + "-" + originalPayment.InspectionId;

                // 5. Tạo payment mới
                var newPayment = new Payment
                {
                    InspectionId = inspectionId,
                    FeeScheduleId = originalPayment.FeeScheduleId,
                    BaseFee = baseFee,
                    CertificateFee = certificateFee,
                    StickerFee = stickerFee,
                    TotalAmount = totalAmount,
                    PaymentMethod = "Chưa xác định",
                    PaymentStatus = 0, // 0 = Chờ thanh toán
                    ReceiptNo = receiptNo,
                    ReceiptPrintCount = 0,
                    CreatedAt = DateTime.Now,
                    PaidAt = null,
                    CreatedBy = originalPayment.CreatedBy,
                    PaidBy = null,
                    Notes = $"Payment bổ sung - Phí {(feePercentage == 0.5m ? "50%" : "100%")} của payment gốc #{originalPayment.PaymentId}",
                    OrderCode = null
                };

                _context.Payments.Add(newPayment);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, $"Tạo payment mới thành công với phí {feePercentage * 100}%", newPayment);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Lỗi: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Tạo payment mới dựa trên paymentId cũ
        /// </summary>
        public async Task<(bool Success, string Message, Payment? NewPayment)> CreateAdditionalPaymentByPaymentIdAsync(int paymentId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // 1. Lấy payment gốc
                var originalPayment = await _context.Payments
                    .Include(p => p.Inspection)
                    .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

                if (originalPayment == null)
                {
                    return (false, "Không tìm thấy payment.", null);
                }

                if (originalPayment.Inspection.IsDeleted)
                {
                    return (false, "Đơn kiểm định đã bị xóa.", null);
                }

                // 2. Tính phí dựa trên ngày tạo của payment gốc
                decimal feePercentage = CalculateFeePercentage(originalPayment.CreatedAt);

                decimal baseFee = originalPayment.BaseFee * feePercentage;
                decimal certificateFee = (originalPayment.CertificateFee ?? 0) * feePercentage;
                decimal stickerFee = (originalPayment.StickerFee ?? 0) * feePercentage;
                decimal totalAmount = baseFee + certificateFee + stickerFee;

                // 3. Tạo ReceiptNo mới
                string datePrefix = DateTime.Now.ToString("yyyyMMddHHmmssfff");                                                                               
                string receiptNo = "RC-" + datePrefix + "-"  + originalPayment.InspectionId;

                // 4. Tạo payment mới
                var newPayment = new Payment
                {
                    InspectionId = originalPayment.InspectionId,
                    FeeScheduleId = originalPayment.FeeScheduleId,
                    BaseFee = baseFee,
                    CertificateFee = certificateFee,
                    StickerFee = stickerFee,
                    TotalAmount = totalAmount,
                    PaymentMethod = "Chưa xác định",
                    PaymentStatus = 0, // 0 = Chờ thanh toán
                    ReceiptNo = receiptNo,
                    ReceiptPrintCount = 0,
                    CreatedAt = DateTime.Now,
                    PaidAt = null,
                    CreatedBy = originalPayment.CreatedBy,
                    PaidBy = null,
                    Notes = $"Payment bổ sung - Phí {(feePercentage == 0.5m ? "50%" : "100%")} của payment #{originalPayment.PaymentId}",
                    OrderCode = null
                };

                _context.Payments.Add(newPayment);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, $"Tạo payment mới thành công với phí {feePercentage * 100}%", newPayment);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Lỗi: {ex.Message}", null);
            }
        }

        /// <summary>
        /// Tính phần trăm phí dựa trên ngày tạo payment gốc
        /// - Cùng ngày: 50%
        /// - Qua ngày: 100%
        /// </summary>
        private decimal CalculateFeePercentage(DateTime originalCreatedAt)
        {
            DateTime today = DateTime.Now.Date;
            DateTime originalDate = originalCreatedAt.Date;

            // Nếu ngày tạo payment gốc = ngày hiện tại → 50%
            if (originalDate == today)
            {
                return 0.5m;
            }

            // Nếu ngày tạo payment gốc < ngày hiện tại → 100%
            if (originalDate < today)
            {
                return 1.0m;
            }

            // Trường hợp ngày tạo > ngày hiện tại (không nên xảy ra)
            return 1.0m;
        }

        /// <summary>
        /// Tạo ReceiptNo mới duy nhất
        /// Format: RCP-YYYYMMDD-XXXX
        /// </summary>
        //private async Task<string> GenerateReceiptNoAsync()
        //{
        //    string datePrefix = DateTime.Now.ToString("yyyyMMddHHmmssfff"); // Sửa logic này
        //    string prefix = $"RC-{datePrefix}-";

        //    // Lấy số thứ tự lớn nhất trong ngày
        //    var lastReceipt = await _context.Payments
        //        .Where(p => p.ReceiptNo!.StartsWith(prefix))
        //        .OrderByDescending(p => p.ReceiptNo)
        //        .Select(p => p.ReceiptNo)
        //        .FirstOrDefaultAsync();

        //    int nextNumber = 1;
        //    if (lastReceipt != null)
        //    {
        //        // Extract số cuối từ "RCP-20250203-0001" → 0001
        //        string lastNumberStr = lastReceipt.Split('-').LastOrDefault() ?? "0000";
        //        if (int.TryParse(lastNumberStr, out int lastNumber))
        //        {
        //            nextNumber = lastNumber + 1;
        //        }
        //    }

        //    return $"{prefix}{nextNumber:D4}"; // RCP-20250203-0001
        //}
    



        public Inspection getInspectionByOrderCode(long? orderCode)
        {
            return _context.Inspections.Include(i => i.Payments).FirstOrDefault(i => i.Payments.Any(p => p.OrderCode == orderCode));
        }


    }
}
