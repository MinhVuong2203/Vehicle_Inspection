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
                query = query.Where(i => i.Payment.PaymentStatus == status.Value);
            }

            // Tìm kiếm theo mã kiểm định hoặc loại kiểm định
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(i =>
                    i.InspectionCode.Contains(search));
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

        // Phục vụ cho thanh toán tiền mặt
        public string CollectPayment(string inspectionCode, string paymentMethod, string? note, Guid userId)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                var inspection = _context.Inspections
                    .Include(i => i.Payment)
                    .FirstOrDefault(i => i.InspectionCode == inspectionCode && !i.IsDeleted);

                if (inspection == null)
                {
                    return "Not found";
                }

                //Kiểm tra trạng thái: chỉ thu phí cho đơn đang chờ(Status = 1)
                // Hoặc đơn đã hoàn thành kiểm định nhưng chưa thanh toán
                if (inspection.Payment != null && inspection.Payment.PaymentStatus == 1)
                {                   
                    // Đã thu phí rồi
                    return "Successed";
                }

                if (inspection.Payment != null && inspection.Payment.PaymentStatus == 2)
                {
                    // Đơn này đã bị hủy
                    return "Failed";
                }

                
                inspection.Payment.PaymentMethod = paymentMethod;
                inspection.Payment.PaymentStatus = 1;
                inspection.Payment.ReceiptPrintCount++;
                inspection.Payment.PaidAt = DateTime.Now;
                inspection.Payment.PaidBy = userId;
                inspection.Payment.Notes = note;
          

                // Cập nhật thông tin Inspection
                inspection.PaidAt = DateTime.Now;

                // Cập nhật Status nếu cần (tùy theo flow nghiệp vụ của bạn)
                // Status = 2 có thể là "Đã thu phí" hoặc "Đã tiếp nhận"
                if (inspection.Status == 1)
                {
                    inspection.Status = 2;
                }

                _context.SaveChanges();
                transaction.Commit();

                return "Success";
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                // Log error
                Console.WriteLine($"Error collecting payment: {ex.Message}");
                return "Errol";
            }
        }
        
        public Inspection getInspectionByOrderCode(long? orderCode)
        {
            return _context.Inspections.Include(i => i.Payment).FirstOrDefault(i => i.Payment.OrderCode == orderCode);
        }


    }
}
