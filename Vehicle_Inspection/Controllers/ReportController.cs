using Microsoft.AspNetCore.Mvc;
using Vehicle_Inspection.Service;

namespace Vehicle_Inspection.Controllers
{
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        public async Task<IActionResult> Index(DateTime? startDate, DateTime? endDate)
        {
            var start = startDate ?? DateTime.Today.AddDays(-30);
            var end = endDate ?? DateTime.Today.AddDays(1).AddSeconds(-1);

            ViewBag.StartDate = start;
            ViewBag.EndDate = end;

            // KPI Overview
            ViewBag.TotalInspections = await _reportService.GetTotalInspectionsAsync(start, end);
            ViewBag.PassedInspections = await _reportService.GetPassedInspectionsAsync(start, end);
            ViewBag.FailedInspections = await _reportService.GetFailedInspectionsAsync(start, end);
            ViewBag.TotalRevenue = await _reportService.GetTotalRevenueAsync(start, end);

            ViewBag.PassRate = ViewBag.TotalInspections > 0
                ? (decimal)ViewBag.PassedInspections / ViewBag.TotalInspections * 100
                : 0;

            // Charts Data
            ViewBag.DailyRevenue = await _reportService.GetDailyRevenueAsync(start, end);
            ViewBag.PaymentMethods = await _reportService.GetPaymentMethodStatsAsync(start, end);
            ViewBag.LaneProduction = await _reportService.GetLaneProductionAsync(start, end);
            //ViewBag.KtvProduction = await _reportService.GetKtvProductionAsync(start, end);
            ViewBag.TopDefects = await _reportService.GetTopDefectsAsync(start, end, 10);
            ViewBag.DefectsByVehicleType = await _reportService.GetDefectsByVehicleTypeAsync(start, end);
            //ViewBag.InspectionTypes = await _reportService.GetInspectionTypeStatsAsync(start, end);

            return View();
        }
    }
}