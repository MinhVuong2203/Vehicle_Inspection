using Microsoft.EntityFrameworkCore;
using Vehicle_Inspection.Data;

namespace Vehicle_Inspection.Service
{
    public class ReportService : IReportService
    {
        private readonly VehInsContext _context;

        public ReportService(VehInsContext context)
        {
            _context = context;
        }

        // KPI Overview
        public async Task<int> GetTotalInspectionsAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Inspections
                .Where(i => i.CreatedAt >= startDate && i.CreatedAt <= endDate && !i.IsDeleted)
                .CountAsync();
        }

        public async Task<int> GetPassedInspectionsAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Inspections
                .Where(i => i.CreatedAt >= startDate && i.CreatedAt <= endDate && i.FinalResult == 1 && !i.IsDeleted)
                .CountAsync();
        }

        public async Task<int> GetFailedInspectionsAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Inspections
                .Where(i => i.CreatedAt >= startDate && i.CreatedAt <= endDate && i.FinalResult == 2 && !i.IsDeleted)
                .CountAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Payments
                .Where(p => p.PaidAt >= startDate && p.PaidAt <= endDate && p.PaymentStatus == 1)
                .SumAsync(p => (decimal?)p.TotalAmount) ?? 0;
        }

        // Doanh thu theo ngày
        public async Task<dynamic> GetDailyRevenueAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Payments
                .Where(p => p.PaidAt >= startDate && p.PaidAt <= endDate && p.PaymentStatus == 1)
                .GroupBy(p => p.PaidAt.Value.Date)
                .Select(g => new
                {
                    Date = g.Key,
                    Amount = g.Sum(p => p.TotalAmount),
                    Count = g.Count()
                })
                .OrderBy(x => x.Date)
                .ToListAsync();
        }

        // Thống kê theo phương thức thanh toán
        public async Task<dynamic> GetPaymentMethodStatsAsync(DateTime startDate, DateTime endDate)
        {
            var total = await _context.Payments
                .Where(p => p.PaidAt >= startDate && p.PaidAt <= endDate && p.PaymentStatus == 1)
                .SumAsync(p => (decimal?)p.TotalAmount) ?? 1;

            return await _context.Payments
                .Where(p => p.PaidAt >= startDate && p.PaidAt <= endDate && p.PaymentStatus == 1)
                .GroupBy(p => p.PaymentMethod)
                .Select(g => new
                {
                    Method = g.Key,
                    Amount = g.Sum(p => p.TotalAmount),
                    Count = g.Count(),
                    Percentage = total > 0 ? (g.Sum(p => p.TotalAmount) / total * 100) : 0
                })
                .ToListAsync();
        }

        // Sản lượng theo Lane
        public async Task<dynamic> GetLaneProductionAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Inspections
                .Where(i => i.CreatedAt >= startDate && i.CreatedAt <= endDate && i.LaneId != null && !i.IsDeleted)
                .Include(i => i.Lane)
                .GroupBy(i => i.Lane.LaneName)
                .Select(g => new
                {
                    LaneName = g.Key,
                    TotalCount = g.Count(),
                    PassedCount = g.Count(i => i.FinalResult == 1),
                    FailedCount = g.Count(i => i.FinalResult == 2),
                    PassRate = g.Count() > 0 ? (decimal)g.Count(i => i.FinalResult == 1) / g.Count() * 100 : 0
                })
                .OrderByDescending(x => x.TotalCount)
                .ToListAsync();
        }

        // Sản lượng theo KTV
        //public async Task<dynamic> GetKtvProductionAsync(DateTime startDate, DateTime endDate)
        //{
        //    return await _context.InspectionStages
        //        .Where(s => s.Inspection.CreatedAt >= startDate && s.Inspection.CreatedAt <= endDate && s.AssignedUserId != null)
        //        .Include(s => s.AssignedUser)
        //        .GroupBy(s => new { s.AssignedUserId, s.AssignedUser.FullName })
        //        .Select(g => new
        //        {
        //            UserId = g.Key.AssignedUserId,
        //            FullName = g.Key.FullName,
        //            TotalStages = g.Count(),
        //            CompletedStages = g.Count(s => s.Status == 2),
        //            PassedStages = g.Count(s => s.StageResult == 1),
        //            FailedStages = g.Count(s => s.StageResult == 2),
        //            CompletionRate = g.Count() > 0 ? (decimal)g.Count(s => s.Status == 2) / g.Count() * 100 : 0
        //        })
        //        .OrderByDescending(x => x.TotalStages)
        //        .Take(10)
        //        .ToListAsync();
        //}

        // Top lỗi phổ biến
        public async Task<dynamic> GetTopDefectsAsync(DateTime startDate, DateTime endDate, int topCount = 10)
        {
            return await _context.InspectionDefects
                .Where(d => d.Inspection.CreatedAt >= startDate && d.Inspection.CreatedAt <= endDate)
                .GroupBy(d => new { d.DefectCategory, d.DefectCode, d.Severity })
                .Select(g => new
                {
                    Category = g.Key.DefectCategory,
                    Code = g.Key.DefectCode,
                    Severity = g.Key.Severity,
                    Count = g.Count(),
                    SeverityText = g.Key.Severity == 1 ? "Khuyết điểm" : g.Key.Severity == 2 ? "Hư hỏng" : "Nguy hiểm"
                })
                .OrderByDescending(x => x.Count)
                .Take(topCount)
                .ToListAsync();
        }

        // Lỗi theo loại xe
        public async Task<dynamic> GetDefectsByVehicleTypeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.InspectionDefects
                .Where(d => d.Inspection.CreatedAt >= startDate && d.Inspection.CreatedAt <= endDate)
                .Include(d => d.Inspection.Vehicle.VehicleType)
                .GroupBy(d => d.Inspection.Vehicle.VehicleType.TypeName)
                .Select(g => new
                {
                    VehicleType = g.Key ?? "Chưa phân loại",
                    TotalDefects = g.Count(),
                    CriticalDefects = g.Count(d => d.Severity == 3),
                    MajorDefects = g.Count(d => d.Severity == 2),
                    MinorDefects = g.Count(d => d.Severity == 1)
                })
                .OrderByDescending(x => x.TotalDefects)
                .ToListAsync();
        }

        //// Thống kê theo loại kiểm định
        //public async Task<dynamic> GetInspectionTypeStatsAsync(DateTime startDate, DateTime endDate)
        //{
        //    var inspections = await _context.Inspections
        //        .Where(i => i.CreatedAt >= startDate && i.CreatedAt <= endDate && !i.IsDeleted)
        //        .Include(i => i.Payment)
        //        .ToListAsync();

        //    return inspections
        //        .GroupBy(i => i.InspectionType)
        //        .Select(g => new
        //        {
        //            Type = g.Key,
        //            TypeName = g.Key == "FIRST" ? "Lần đầu" : g.Key == "PERIODIC" ? "Định kỳ" : "Tái kiểm",
        //            Count = g.Count(),
        //            Revenue = g.Sum(i => i.Payment?.TotalAmount ?? 0),
        //            PassedCount = g.Count(i => i.FinalResult == 1),
        //            FailedCount = g.Count(i => i.FinalResult == 2)
        //        })
        //        .OrderByDescending(x => x.Count)
        //        .ToList();
        //}
    }
}