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

            var lanes = _inspectionService.GetInspectionLanes();
            ViewData["Lanes"] = lanes;

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

        [HttpGet]
        public IActionResult GetInspectionStages(int inspectionId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Getting stages for inspection: {inspectionId}");

                var stages = _inspectionService.GetInspectionStages(inspectionId);

                System.Diagnostics.Debug.WriteLine($"Found {stages.Count} stages");

                return Json(new { success = true, data = stages }, _jsonOptions);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = ex.Message }, _jsonOptions);
            }
        }

        // API: POST /Inspection/SaveStageResult (lưu kết quả công đoạn)
        [HttpPost]
        public IActionResult SaveStageResult([FromBody] SaveStageResultRequest request)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Saving stage result for InspStageId: {request.InspStageId}");

                if (request.Measurements == null || request.Measurements.Count == 0)
                {
                    return Json(new { success = false, message = "Không có dữ liệu đo" }, _jsonOptions);
                }

                var success = _inspectionService.SaveStageResult(request);

                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Đã lưu kết quả công đoạn thành công"
                    }, _jsonOptions);
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không thể lưu kết quả"
                    }, _jsonOptions);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return Json(new
                {
                    success = false,
                    message = $"Lỗi: {ex.Message}"
                }, _jsonOptions);
            }
        }

        //lấy danh sách lỗi của công đoạn
        [HttpGet]
        public IActionResult GetStageDefects(int inspectionId, int stageId)
        {
            try
            {
                var defects = _inspectionService.GetStageDefects(inspectionId, stageId);
                return Json(new { success = true, data = defects }, _jsonOptions);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return Json(new { success = false, message = ex.Message }, _jsonOptions);
            }
        }

        // API: POST /Inspection/SubmitInspectionResult (nộp kết quả kiểm định)
        [HttpPost]
        public IActionResult SubmitInspectionResult([FromBody] SubmitInspectionResultRequest request)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Submitting inspection result for InspectionId: {request.InspectionId}");
                System.Diagnostics.Debug.WriteLine($"FinalResult: {request.FinalResult}");

                if (request.FinalResult == null || request.FinalResult < 1 || request.FinalResult > 3)
                {
                    return Json(new { success = false, message = "Vui lòng chọn kết luận cuối cùng!" }, _jsonOptions);
                }

                var success = _inspectionService.SubmitInspectionResult(request);

                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Đã hoàn thành kiểm định thành công!"
                    }, _jsonOptions);
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không thể hoàn thành kiểm định"
                    }, _jsonOptions);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return Json(new
                {
                    success = false,
                    message = $"Lỗi: {ex.Message}"
                }, _jsonOptions);
            }
        }

        //luu dây chuyền kiểm định cho hồ sơ
        [HttpPost]
        public IActionResult AssignLane([FromBody] AssignLaneRequest request)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Assigning lane for InspectionId: {request.InspectionId}");
                System.Diagnostics.Debug.WriteLine($"LaneId: {request.LaneId}");

                if (request.InspectionId <= 0 || request.LaneId <= 0)
                {
                    return Json(new { success = false, message = "Thông tin không hợp lệ" }, _jsonOptions);
                }

                var success = _inspectionService.AssignLane(request);

                if (success)
                {
                    return Json(new
                    {
                        success = true,
                        message = "Đã phân công dây chuyền thành công!"
                    }, _jsonOptions);
                }
                else
                {
                    return Json(new
                    {
                        success = false,
                        message = "Không thể phân công dây chuyền. Vui lòng kiểm tra lại trạng thái hồ sơ."
                    }, _jsonOptions);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return Json(new
                {
                    success = false,
                    message = $"Lỗi: {ex.Message}"
                }, _jsonOptions);
            }
        }
    }
}
