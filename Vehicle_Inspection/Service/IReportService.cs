namespace Vehicle_Inspection.Service
{
    public interface IReportService
    {
        // KPI Overview
        Task<int> GetTotalInspectionsAsync(DateTime startDate, DateTime endDate);
        Task<int> GetPassedInspectionsAsync(DateTime startDate, DateTime endDate);
        Task<int> GetFailedInspectionsAsync(DateTime startDate, DateTime endDate);
        Task<decimal> GetTotalRevenueAsync(DateTime startDate, DateTime endDate);

        // Doanh thu
        Task<dynamic> GetDailyRevenueAsync(DateTime startDate, DateTime endDate);
        Task<dynamic> GetPaymentMethodStatsAsync(DateTime startDate, DateTime endDate);

        // Sản lượng
        Task<dynamic> GetLaneProductionAsync(DateTime startDate, DateTime endDate);
        Task<dynamic> GetKtvProductionAsync(DateTime startDate, DateTime endDate);

        // Lỗi
        Task<dynamic> GetTopDefectsAsync(DateTime startDate, DateTime endDate, int topCount = 10);
        Task<dynamic> GetDefectsByVehicleTypeAsync(DateTime startDate, DateTime endDate);

        // Loại kiểm định
        Task<dynamic> GetInspectionTypeStatsAsync(DateTime startDate, DateTime endDate);
    }
}