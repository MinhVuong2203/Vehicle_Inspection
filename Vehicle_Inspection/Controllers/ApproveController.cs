using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vehicle_Inspection.Data;
using Vehicle_Inspection.Models;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Vehicle_Inspection.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ApproveController : ControllerBase
    {
        private readonly VehInsContext _context;

        public ApproveController(VehInsContext context)
        {
            _context = context;
        }

        /// <summary>
        /// XÉT DUYỆT HỒ SƠ - LOGIC ĐƠN GIẢN
        /// 
        /// LOGIC XÁC ĐỊNH InspectionType:
        /// - Chưa có hồ sơ nào → FIRST
        /// - Đã có hồ sơ → PERIODIC
        /// 
        /// LOGIC XÁC ĐỊNH ACTION:
        /// - Status = 6 (Không đạt) → UPDATE hồ sơ hiện tại
        ///   • Count_Re < 3 hoặc NULL → Status = 2 (miễn phí)
        ///   • Count_Re >= 3 → Status = 1 (có phí)
        /// - Status = 7 (Đã cấp GCN) → CREATE hồ sơ mới
        /// - Chưa có hồ sơ → CREATE hồ sơ mới
        /// </summary>
        [HttpPost("approve")]
        public async Task<IActionResult> ApproveInspection([FromBody] ApproveInspectionRequest request)
        {
            try
            {
                Console.WriteLine($"💾 ========== BẮT ĐẦU XÉT DUYỆT ==========");
                Console.WriteLine($"📋 VehicleId: {request.VehicleId}");
                Console.WriteLine($"👤 OwnerId: {request.OwnerId}");

                // Validate input
                if (request.VehicleId <= 0 || request.OwnerId == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Thông tin không hợp lệ"
                    });
                }

                // Lấy hồ sơ kiểm định mới nhất
                var latestInspection = await _context.Inspections
                    .Where(i => i.VehicleId == request.VehicleId && i.IsDeleted == false)
                    .OrderByDescending(i => i.CreatedAt)
                    .FirstOrDefaultAsync();

                string action;
                string inspectionType;
                int resultInspectionId;
                string resultInspectionCode;
                int? resultCountRe = 0;
                short resultStatus;

                // ========== TRƯỜNG HỢP 1: CHƯA CÓ HỒ SƠ NÀO ==========
                if (latestInspection == null)
                {
                    action = "CREATE";
                    inspectionType = "FIRST";
                    resultStatus = 1; // RECEIVED

                    Console.WriteLine("📌 Chưa có hồ sơ → TẠO MỚI với FIRST");

                    // Tạo inspection code nếu không có
                    var inspectionCode = string.IsNullOrWhiteSpace(request.InspectionCode)
                        ? GenerateInspectionCode()
                        : request.InspectionCode;

                    // Tạo hồ sơ mới
                    var newInspection = new Inspection
                    {
                        InspectionCode = inspectionCode,
                        VehicleId = request.VehicleId,
                        OwnerId = request.OwnerId,
                        InspectionType = inspectionType,
                        Status = resultStatus,
                        Count_Re = 0,
                        Notes = request.Notes,
                        CreatedAt = DateTime.Now,
                        IsDeleted = false
                    };

                    _context.Inspections.Add(newInspection);
                    await _context.SaveChangesAsync();

                    resultInspectionId = newInspection.InspectionId;
                    resultInspectionCode = newInspection.InspectionCode;
                    resultCountRe = 0;

                    Console.WriteLine($"✅ Tạo hồ sơ mới thành công: {resultInspectionCode}");
                }
                // ========== TRƯỜNG HỢP 2: STATUS = 6 (KHÔNG ĐẠT) → UPDATE ==========
                else if (latestInspection.Status == 6)
                {
                    action = "UPDATE";
                    inspectionType = latestInspection.InspectionType; // Giữ nguyên type

                    Console.WriteLine($"📌 Hồ sơ {latestInspection.InspectionCode} không đạt → CẬP NHẬT");

                    // Tăng Count_Re
                    int currentCountRe = latestInspection.Count_Re ?? 0;
                    int newCountRe = currentCountRe + 1;
                    latestInspection.Count_Re = newCountRe;

                    // Xác định Status mới
                    if (newCountRe < 3)
                    {
                        latestInspection.Status = 2; // APPROVED - Miễn phí
                        resultStatus = 2;
                        Console.WriteLine($"   → Count_Re = {newCountRe} (< 3) → Status = 2 (Miễn phí)");
                    }
                    else
                    {
                        latestInspection.Status = 1; // RECEIVED - Có phí
                        resultStatus = 1;
                        Console.WriteLine($"   → Count_Re = {newCountRe} (>= 3) → Status = 1 (Có phí)");
                    }

                    // Cập nhật Notes nếu có
                    if (!string.IsNullOrWhiteSpace(request.Notes))
                    {
                        latestInspection.Notes = request.Notes;
                    }

                    await _context.SaveChangesAsync();

                    resultInspectionId = latestInspection.InspectionId;
                    resultInspectionCode = latestInspection.InspectionCode;
                    resultCountRe = latestInspection.Count_Re;

                    Console.WriteLine($"✅ Cập nhật thành công: Count_Re={resultCountRe}, Status={resultStatus}");
                }
                // ========== TRƯỜNG HỢP 3: STATUS = 7 (ĐÃ CẤP GCN) → CREATE ==========
                else if (latestInspection.Status == 7)
                {
                    action = "CREATE";
                    inspectionType = "PERIODIC";
                    resultStatus = 1; // RECEIVED

                    Console.WriteLine("📌 Hồ sơ trước đã cấp GCN → TẠO MỚI với PERIODIC");

                    // Tạo inspection code nếu không có
                    var inspectionCode = string.IsNullOrWhiteSpace(request.InspectionCode)
                        ? GenerateInspectionCode()
                        : request.InspectionCode;

                    // Tạo hồ sơ mới
                    var newInspection = new Inspection
                    {
                        InspectionCode = inspectionCode,
                        VehicleId = request.VehicleId,
                        OwnerId = request.OwnerId,
                        InspectionType = inspectionType,
                        Status = resultStatus,
                        Count_Re = 0,
                        Notes = request.Notes,
                        CreatedAt = DateTime.Now,
                        IsDeleted = false
                    };

                    _context.Inspections.Add(newInspection);
                    await _context.SaveChangesAsync();

                    resultInspectionId = newInspection.InspectionId;
                    resultInspectionCode = newInspection.InspectionCode;
                    resultCountRe = 0;

                    Console.WriteLine($"✅ Tạo hồ sơ mới thành công: {resultInspectionCode}");
                }
                // ========== TRƯỜNG HỢP 4: STATUS KHÁC (0,1,2,3,4,5,8) ==========
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Hồ sơ đang ở trạng thái {latestInspection.Status}, không thể xét duyệt. Vui lòng hoàn thành quy trình hiện tại."
                    });
                }

                Console.WriteLine($"✅ XÉT DUYỆT HOÀN TẤT:");
                Console.WriteLine($"   - Action: {action}");
                Console.WriteLine($"   - InspectionType: {inspectionType}");
                Console.WriteLine($"   - InspectionCode: {resultInspectionCode}");
                Console.WriteLine($"   - Status: {resultStatus}");
                Console.WriteLine($"   - Count_Re: {resultCountRe}");

                return Ok(new
                {
                    success = true,
                    message = action == "CREATE" ? "Tạo hồ sơ kiểm định mới thành công" : "Cập nhật hồ sơ tái kiểm thành công",
                    data = new
                    {
                        action = action,
                        inspectionId = resultInspectionId,
                        inspectionCode = resultInspectionCode,
                        inspectionType = inspectionType,
                        status = resultStatus,
                        countRe = resultCountRe
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");

                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi xét duyệt hồ sơ",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// LẤY THÔNG TIN HỒ SƠ MỚI NHẤT (để hiển thị UI)
        /// </summary>
        [HttpGet("latest")]
        public async Task<IActionResult> GetLatestInspection([FromQuery] int vehicleId)
        {
            try
            {
                if (vehicleId <= 0)
                {
                    return BadRequest(new { success = false, message = "VehicleId không hợp lệ" });
                }

                var latestInspection = await _context.Inspections
                    .Where(i => i.VehicleId == vehicleId && i.IsDeleted == false)
                    .OrderByDescending(i => i.CreatedAt)
                    .FirstOrDefaultAsync();

                if (latestInspection == null)
                {
                    return Ok(new
                    {
                        success = true,
                        data = new
                        {
                            hasInspection = false,
                            inspectionType = "FIRST",
                            action = "CREATE",
                            message = "Xe chưa có hồ sơ kiểm định → Tạo mới với FIRST"
                        }
                    });
                }

                // Lấy thông tin Vehicle
                var vehicle = await _context.Vehicles.FindAsync(latestInspection.VehicleId);

                // Xác định action và type
                string action;
                string inspectionType;
                string message;

                if (latestInspection.Status == 6)
                {
                    action = "UPDATE";
                    inspectionType = latestInspection.InspectionType;
                    message = $"Hồ sơ {latestInspection.InspectionCode} không đạt → Cập nhật để tái kiểm";
                }
                else if (latestInspection.Status == 7)
                {
                    action = "CREATE";
                    inspectionType = "PERIODIC";
                    message = "Xe đã cấp GCN → Tạo mới với PERIODIC";
                }
                else
                {
                    action = "NONE";
                    inspectionType = latestInspection.InspectionType;
                    message = $"Hồ sơ đang ở trạng thái {latestInspection.Status}";
                }

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        hasInspection = true,
                        action = action,
                        inspectionType = inspectionType,
                        message = message,
                        latestInspection = new
                        {
                            latestInspection.InspectionId,
                            latestInspection.InspectionCode,
                            latestInspection.InspectionType,
                            latestInspection.Status,
                            latestInspection.CreatedAt,
                            latestInspection.Count_Re,
                            latestInspection.Notes,
                            VehiclePlate = vehicle?.PlateNo
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra", error = ex.Message });
            }
        }

        /// <summary>
        /// LẤY LỊCH SỬ KIỂM ĐỊNH CỦA XE
        /// </summary>
        [HttpGet("history")]
        public async Task<IActionResult> GetInspectionHistory([FromQuery] int vehicleId)
        {
            try
            {
                if (vehicleId <= 0)
                {
                    return BadRequest(new { success = false, message = "VehicleId không hợp lệ" });
                }

                var history = await _context.Inspections
                    .Where(i => i.VehicleId == vehicleId && i.IsDeleted == false)
                    .OrderByDescending(i => i.CreatedAt)
                    .Take(5)
                    .ToListAsync();

                var result = history.Select(i => new
                {
                    i.InspectionId,
                    i.InspectionCode,
                    i.InspectionType,
                    i.Status,
                    i.CreatedAt,
                    i.Count_Re,
                    StatusText = i.Status == 0 ? "Pending" :
                               i.Status == 1 ? "Received" :
                               i.Status == 2 ? "Approved" :
                               i.Status == 3 ? "In Progress" :
                               i.Status == 4 ? "Completed" :
                               i.Status == 5 ? "Passed" :
                               i.Status == 6 ? "Failed" :
                               i.Status == 7 ? "Certified" : "Unknown"
                }).ToList();

                return Ok(new { success = true, count = result.Count, data = result });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                return StatusCode(500, new { success = false, message = "Có lỗi xảy ra", error = ex.Message });
            }
        }

        /// <summary>
        /// Tự động sinh mã kiểm định
        /// </summary>
        private string GenerateInspectionCode()
        {
            var now = DateTime.Now;
            return $"INS-{now:yyyyMMdd}-{now:HHmmss}";
        }
    }

    // DTO for Approve Inspection Request
    public class ApproveInspectionRequest
    {
        public int VehicleId { get; set; }
        public Guid OwnerId { get; set; }
        public string? InspectionCode { get; set; } // Tùy chọn, nếu không có sẽ tự sinh
        public string? Notes { get; set; }
    }
}