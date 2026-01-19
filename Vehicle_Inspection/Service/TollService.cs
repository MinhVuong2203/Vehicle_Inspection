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

            public List<Inspection> GetInspections(string? search, short? status)
            {
                var query = _context.Inspections
                    .Include(i => i.Vehicle)
                    
                .Include(i => i.Owner)
                .Include(i => i.Payment)
                .Where(i => !i.IsDeleted);

            // Lọc theo trạng thái
            if (status.HasValue)
            {
                query = query.Where(i => i.Status == status.Value);
            }

            // Tìm kiếm theo mã kiểm định hoặc loại kiểm định
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(i =>
                    i.InspectionCode.Contains(search) ||
                    i.InspectionType.Contains(search));
            }

            return query.OrderByDescending(i => i.CreatedAt).ToList();
        }

        public Inspection? GetInspectionDetails(string inspectionCode)
        {
            return _context.Inspections
                .Include(i => i.Vehicle)
                .Include(i => i.Owner)
                .Include(i => i.Payment)
                .FirstOrDefault(i => i.InspectionCode == inspectionCode && !i.IsDeleted);
        }

        public bool CollectPayment(string inspectionCode, string paymentMethod, string? note, Guid userId)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var inspection = _context.Inspections
                    .Include(i => i.Payment)
                    .FirstOrDefault(i => i.InspectionCode == inspectionCode && !i.IsDeleted);

                if (inspection == null)
                {
                    return false;
                }

                // Kiểm tra trạng thái: chỉ thu phí cho đơn đang chờ (Status = 1)
                // Hoặc đơn đã hoàn thành kiểm định nhưng chưa thanh toán
                //if (inspection.Payment != null && inspection.Payment.Status == 1)
                //{
                //    // Đã thu phí rồi
                //    return false;
                //}

                //// Cập nhật hoặc tạo mới Payment
                //if (inspection.Payment == null)
                //{
                //    var payment = new Payment
                //    {
                //        InspectionId = inspection.InspectionId,
                //        Amount = 500000, // Có thể tính toán dựa vào loại kiểm định
                //        PaymentMethod = paymentMethod,
                //        Status = 1, // 1 = Đã thanh toán
                //        PaymentDate = DateTime.Now,
                //        Note = note,
                //        CreatedBy = userId,
                //        CreatedAt = DateTime.Now
                //    };
                //    _context.Payments.Add(payment);
                //}
                //else
                //{
                //    inspection.Payment.PaymentMethod = paymentMethod;
                //    inspection.Payment.Status = 1;
                //    inspection.Payment.PaymentDate = DateTime.Now;
                //    inspection.Payment.Note = note;
                //}

                // Cập nhật thông tin Inspection
                inspection.PaidAt = DateTime.Now;
                inspection.ReceivedBy = userId;
                inspection.ReceivedAt = DateTime.Now;

                // Cập nhật Status nếu cần (tùy theo flow nghiệp vụ của bạn)
                // Status = 2 có thể là "Đã thu phí" hoặc "Đã tiếp nhận"
                if (inspection.Status == 1)
                {
                    inspection.Status = 2;
                }

                _context.SaveChanges();
                transaction.Commit();

                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                // Log error
                Console.WriteLine($"Error collecting payment: {ex.Message}");
                return false;
            }
        }
    }
}
