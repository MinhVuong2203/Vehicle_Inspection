using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Vehicle_Inspection.Service;

namespace Vehicle_Inspection.Controllers
{
    public class InspectionController : Controller
    {
        private readonly IInspectionService _inspectionService;
        private readonly JsonSerializerOptions _jsonOptions;

        public InspectionController(IInspectionService inspectionService)
        {
            _inspectionService = inspectionService;

            // Configure JSON để trả về camelCase
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }
    
        public IActionResult Index()
        {
            var inspectionRecords = _inspectionService.GetInspectionRecords();
            ViewData["InspectionRecords"] = inspectionRecords;
            TempData["SuccessMessage"] = "Dữ liệu hồ sơ kiểm định đã được tải thành công.";
            return View();
        }

        // API: GET /Inspection/GetInspectionRecords
        [HttpGet]
        public IActionResult GetInspectionRecords()
        {
            try
            {
                var records = _inspectionService.GetInspectionRecords();

                System.Diagnostics.Debug.WriteLine($"Found {records?.Count ?? 0} inspection records");

                return Json(new { success = true, data = records }, _jsonOptions);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetInspectionRecords: {ex.Message}");
                return Json(new { success = false, message = ex.Message }, _jsonOptions);
            }
        }

        [HttpGet]
        public IActionResult GetInspectionRecord(int id)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Getting inspection detail for ID: {id}");

                var record = _inspectionService.GetInspectionDetail(id);

                if (record == null)
                {
                    System.Diagnostics.Debug.WriteLine($"Inspection {id} not found");
                    return Json(new { success = false, message = "Không tìm thấy hồ sơ" }, _jsonOptions);
                }

                System.Diagnostics.Debug.WriteLine($"Found inspection: {record.InspectionCode}");
                System.Diagnostics.Debug.WriteLine($"WheelFormula: {record.WheelFormula}");
                System.Diagnostics.Debug.WriteLine($"KerbWeight: {record.KerbWeight}");

                return Json(new { success = true, data = record }, _jsonOptions);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error in GetInspectionRecord: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"StackTrace: {ex.StackTrace}");
                return Json(new { success = false, message = ex.Message }, _jsonOptions);
            }
        }
    }
}
