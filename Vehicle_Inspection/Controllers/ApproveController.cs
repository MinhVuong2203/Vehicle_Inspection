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
        /// TỰ ĐỘNG PHÁT HIỆN LOẠI KIỂM ĐỊNH DỰA TRÊN HỒ SƠ MỚI NHẤT
        /// Logic:
        /// - Nếu chưa có lịch sử → FIRST
        /// - Nếu hồ sơ mới nhất Status = 7 (Đã cấp GCN) → PERIODIC (sẽ tạo hồ sơ mới)
        /// - Nếu hồ sơ mới nhất Status = 6 (Không đạt) → RE_INSPECTION (sẽ cập nhật hồ sơ đó)
        /// - Nếu hồ sơ mới nhất Status = 5 (Đạt) → PERIODIC (sẽ tạo hồ sơ mới)
        /// </summary>
        [HttpGet("detect-type")]
        public async Task<IActionResult> DetectInspectionType([FromQuery] int vehicleId)
        {
            try
            {
                Console.WriteLine($"🔍 ========== PHÁT HIỆN LOẠI KIỂM ĐỊNH ==========");
                Console.WriteLine($"📋 VehicleId: {vehicleId}");

                if (vehicleId <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "VehicleId không hợp lệ"
                    });
                }

                // Lấy hồ sơ kiểm định mới nhất của xe (dựa vào CreatedAt)
                var latestInspection = await _context.Inspections
                    .Where(i => i.VehicleId == vehicleId && i.IsDeleted == false)
                    .OrderByDescending(i => i.CreatedAt)
                    .Select(i => new
                    {
                        i.InspectionId,
                        i.InspectionCode,
                        i.InspectionType,
                        i.Status,
                        i.CreatedAt,
                        i.Count_Re
                    })
                    .FirstOrDefaultAsync();

                string detectedType = "FIRST";
                string reason = "";
                string action = "CREATE"; // CREATE hoặc UPDATE
                int? targetInspectionId = null;
                int? latestCountRe = null;

                if (latestInspection == null)
                {
                    // ✅ TRƯỜNG HỢP 1: Chưa có hồ sơ nào → FIRST → TẠO MỚI
                    detectedType = "FIRST";
                    reason = "Xe chưa từng kiểm định trước đó";
                    action = "CREATE";
                    Console.WriteLine("✅ Kết quả: FIRST (Chưa có lịch sử) → TẠO MỚI");
                }
                else
                {
                    Console.WriteLine($"🔍 Hồ sơ mới nhất: Code={latestInspection.InspectionCode}, Status={latestInspection.Status}, Count_Re={latestInspection.Count_Re}");
                    latestCountRe = latestInspection.Count_Re;
                    targetInspectionId = latestInspection.InspectionId;

                    if (latestInspection.Status == 6)
                    {
                        // ✅ TRƯỜNG HỢP 2: Hồ sơ mới nhất KHÔNG ĐẠT → RE_INSPECTION → CẬP NHẬT
                        detectedType = "RE_INSPECTION";
                        reason = $"Hồ sơ mới nhất ({latestInspection.InspectionCode}) không đạt, cần tái kiểm";
                        action = "UPDATE";
                        Console.WriteLine($"✅ Kết quả: RE_INSPECTION (Status = 6) → CẬP NHẬT hồ sơ {latestInspection.InspectionId}");
                    }
                    else if (latestInspection.Status == 7)
                    {
                        // ✅ TRƯỜNG HỢP 3: Hồ sơ mới nhất ĐÃ CẤP GCN → PERIODIC → TẠO MỚI
                        detectedType = "PERIODIC";
                        reason = $"Xe đã có giấy chứng nhận ({latestInspection.InspectionCode}), thực hiện kiểm định định kỳ";
                        action = "CREATE";
                        targetInspectionId = null;
                        Console.WriteLine($"✅ Kết quả: PERIODIC (Status = 7) → TẠO MỚI");
                    }
                    else if (latestInspection.Status == 5)
                    {
                        // ✅ TRƯỜNG HỢP 4: Hồ sơ mới nhất ĐẠT → PERIODIC → TẠO MỚI
                        detectedType = "PERIODIC";
                        reason = $"Lượt kiểm định trước đã đạt ({latestInspection.InspectionCode}), thực hiện kiểm định định kỳ";
                        action = "CREATE";
                        targetInspectionId = null;
                        Console.WriteLine($"✅ Kết quả: PERIODIC (Status = 5) → TẠO MỚI");
                    }
                    else
                    {
                        // ✅ TRƯỜNG HỢP 5: Status khác (0,1,2,3,4,8) → Tìm lượt hoàn thành gần nhất
                        Console.WriteLine($"⚠️ Status đặc biệt: {latestInspection.Status}, tìm lượt hoàn thành gần nhất...");

                        var completedInspection = await _context.Inspections
                            .Where(i => i.VehicleId == vehicleId && i.IsDeleted == false && (i.Status == 5 || i.Status == 6 || i.Status == 7))
                            .OrderByDescending(i => i.CreatedAt)
                            .Select(i => new { i.InspectionId, i.InspectionCode, i.Status })
                            .FirstOrDefaultAsync();

                        if (completedInspection != null)
                        {
                            if (completedInspection.Status == 7 || completedInspection.Status == 5)
                            {
                                detectedType = "PERIODIC";
                                reason = "Xe đã có lượt kiểm định hoàn thành trước đó, thực hiện kiểm định định kỳ";
                                action = "CREATE";
                                targetInspectionId = null;
                            }
                            else if (completedInspection.Status == 6)
                            {
                                detectedType = "RE_INSPECTION";
                                reason = "Lượt kiểm định trước đó không đạt, cần tái kiểm";
                                action = "UPDATE";
                                targetInspectionId = completedInspection.InspectionId;
                            }
                        }
                        else
                        {
                            detectedType = "FIRST";
                            reason = "Chưa có lượt kiểm định hoàn thành, coi như lần đầu";
                            action = "CREATE";
                            targetInspectionId = null;
                        }
                    }
                }

                Console.WriteLine($"✅ Kết luận cuối cùng: {detectedType} - {action}");
                Console.WriteLine($"📝 Lý do: {reason}");

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        inspectionType = detectedType,
                        reason = reason,
                        action = action, // CREATE hoặc UPDATE
                        targetInspectionId = targetInspectionId, // ID hồ sơ cần cập nhật (nếu UPDATE)
                        latestInspection = latestInspection,
                        latestCountRe = latestCountRe
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi phát hiện loại kiểm định",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// XÉT DUYỆT HỒ SƠ - LOGIC MỚI
        /// Xét InspectionType của hồ sơ MỚI NHẤT trước khi cập nhật:
        /// - Nếu hồ sơ mới nhất là PERIODIC → chuyển thành RE_INSPECTION, KHÔNG tăng Count_Re
        /// - Nếu hồ sơ mới nhất là RE_INSPECTION → giữ nguyên RE_INSPECTION, CÓ tăng Count_Re
        /// - Nếu Status = 7 (Đã cấp GCN) hoặc 5 (Đạt) → TẠO HỒ SƠ MỚI
        /// </summary>
        [HttpPost("approve")]
        public async Task<IActionResult> ApproveInspection([FromBody] ApproveInspectionRequest request)
        {
            try
            {
                Console.WriteLine("💾 ========== BẮT ĐẦU XÉT DUYỆT HỒ SƠ ==========");
                Console.WriteLine($"📋 VehicleId: {request.VehicleId}");
                Console.WriteLine($"📋 OwnerId: {request.OwnerId}");
                Console.WriteLine($"📋 InspectionType từ request: {request.InspectionType}");

                // Validate dữ liệu
                if (request.VehicleId <= 0)
                {
                    return BadRequest(new { success = false, message = "VehicleId không hợp lệ" });
                }

                if (request.OwnerId == Guid.Empty)
                {
                    return BadRequest(new { success = false, message = "OwnerId không hợp lệ" });
                }

                if (string.IsNullOrWhiteSpace(request.InspectionType))
                {
                    return BadRequest(new { success = false, message = "Loại kiểm định không hợp lệ" });
                }

                // Lấy hồ sơ mới nhất (dựa vào CreatedAt)
                var latestInspection = await _context.Inspections
                    .Where(i => i.VehicleId == request.VehicleId && i.IsDeleted == false)
                    .OrderByDescending(i => i.CreatedAt)
                    .FirstOrDefaultAsync();

                string action = "";
                int resultInspectionId = 0;
                string resultInspectionCode = "";
                int? resultCountRe = null;

                // ========== TRƯỜNG HỢP 1: TẠO MỚI ==========
                if (latestInspection == null || latestInspection.Status == 7 || latestInspection.Status == 5)
                {
                    action = "CREATE";
                    Console.WriteLine($"🆕 TẠO HỒ SƠ MỚI");

                    // Generate InspectionCode
                    string inspectionCode = request.InspectionCode;
                    if (string.IsNullOrWhiteSpace(inspectionCode))
                    {
                        inspectionCode = $"KD-{DateTime.Now:yyyyMMdd}-{DateTime.Now:HHmmssfff}";
                    }

                    // Kiểm tra InspectionCode đã tồn tại chưa
                    var existingCode = await _context.Inspections
                        .FirstOrDefaultAsync(i => i.InspectionCode == inspectionCode);

                    if (existingCode != null)
                    {
                        return BadRequest(new { success = false, message = "Mã lượt kiểm định đã tồn tại" });
                    }

                    // Kiểm tra Vehicle và Owner tồn tại
                    var vehicle = await _context.Vehicles.FindAsync(request.VehicleId);
                    if (vehicle == null)
                    {
                        return NotFound(new { success = false, message = "Không tìm thấy thông tin phương tiện" });
                    }

                    var owner = await _context.Owners.FindAsync(request.OwnerId);
                    if (owner == null)
                    {
                        return NotFound(new { success = false, message = "Không tìm thấy thông tin chủ xe" });
                    }

                    // Tạo hồ sơ mới
                    var newInspection = new Inspection
                    {
                        InspectionCode = inspectionCode,
                        VehicleId = request.VehicleId,
                        OwnerId = request.OwnerId,
                        InspectionType = request.InspectionType,
                        LaneId = null,
                        Status = 1, // RECEIVED
                        CreatedAt = DateTime.Now,
                        Notes = request.Notes,
                        IsDeleted = false,
                        Count_Re = null // Hồ sơ mới chưa có tái kiểm
                    };

                    _context.Inspections.Add(newInspection);
                    await _context.SaveChangesAsync();

                    resultInspectionId = newInspection.InspectionId;
                    resultInspectionCode = newInspection.InspectionCode;
                    resultCountRe = newInspection.Count_Re;

                    Console.WriteLine($"✅ Tạo hồ sơ mới thành công: ID={resultInspectionId}, Code={resultInspectionCode}");
                }
                // ========== TRƯỜNG HỢP 2: CẬP NHẬT (Status = 6 - Không đạt) ==========
                else if (latestInspection.Status == 6)
                {
                    action = "UPDATE";
                    Console.WriteLine($"🔄 CẬP NHẬT HỒ SƠ: ID={latestInspection.InspectionId}, Code={latestInspection.InspectionCode}");
                    Console.WriteLine($"   InspectionType hiện tại của hồ sơ: {latestInspection.InspectionType}");
                    Console.WriteLine($"   Count_Re hiện tại: {latestInspection.Count_Re}");

                    // ✅ LOGIC MỚI: Xét InspectionType HIỆN TẠI của hồ sơ để quyết định
                    if (latestInspection.InspectionType == "PERIODIC")
                    {
                        // Nếu hồ sơ hiện tại là PERIODIC → chuyển thành RE_INSPECTION, KHÔNG tăng Count_Re
                        Console.WriteLine($"📌 Hồ sơ hiện tại là PERIODIC → Chuyển thành RE_INSPECTION, KHÔNG tăng Count_Re");
                        latestInspection.InspectionType = "RE_INSPECTION";
                        // Count_Re giữ nguyên (không thay đổi)
                        Console.WriteLine($"   Count_Re giữ nguyên: {latestInspection.Count_Re}");
                    }
                    else if (latestInspection.InspectionType == "RE_INSPECTION")
                    {
                        // Nếu hồ sơ hiện tại là RE_INSPECTION → giữ nguyên RE_INSPECTION, CÓ tăng Count_Re
                        Console.WriteLine($"📌 Hồ sơ hiện tại là RE_INSPECTION → Giữ nguyên RE_INSPECTION, CÓ tăng Count_Re");
                        latestInspection.InspectionType = "RE_INSPECTION";
                        latestInspection.Count_Re = (latestInspection.Count_Re ?? 0) + 1;
                        Console.WriteLine($"   Count_Re tăng lên: {latestInspection.Count_Re}");
                    }
                    else
                    {
                        // Các trường hợp khác (FIRST, v.v.) → chuyển thành RE_INSPECTION, không tăng Count_Re
                        Console.WriteLine($"📌 Hồ sơ hiện tại là {latestInspection.InspectionType} → Chuyển thành RE_INSPECTION, KHÔNG tăng Count_Re");
                        latestInspection.InspectionType = "RE_INSPECTION";
                        // Count_Re giữ nguyên
                        Console.WriteLine($"   Count_Re giữ nguyên: {latestInspection.Count_Re}");
                    }

                    // Cập nhật Status
                    latestInspection.Status = 1; // RECEIVED - Quay về trạng thái tiếp nhận để kiểm định lại

                    // Cập nhật Notes (nếu có)
                    if (!string.IsNullOrWhiteSpace(request.Notes))
                    {
                        latestInspection.Notes = request.Notes;
                    }

                    await _context.SaveChangesAsync();

                    resultInspectionId = latestInspection.InspectionId;
                    resultInspectionCode = latestInspection.InspectionCode;
                    resultCountRe = latestInspection.Count_Re;

                    Console.WriteLine($"✅ Cập nhật hồ sơ thành công: ID={resultInspectionId}, Type={latestInspection.InspectionType}, Count_Re={resultCountRe}");
                }
                // ========== TRƯỜNG HỢP 3: Status khác (0,1,2,3,4,8) ==========
                else
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"Hồ sơ mới nhất đang ở trạng thái {latestInspection.Status}, không thể xét duyệt. Vui lòng hoàn thành quy trình hiện tại trước."
                    });
                }

                Console.WriteLine($"✅ XÉT DUYỆT HOÀN TẤT:");
                Console.WriteLine($"   - Action: {action}");
                Console.WriteLine($"   - InspectionId: {resultInspectionId}");
                Console.WriteLine($"   - InspectionCode: {resultInspectionCode}");
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
                        inspectionType = latestInspection != null ? latestInspection.InspectionType : request.InspectionType,
                        status = 1, // RECEIVED
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
        /// LẤY THÔNG TIN HỒ SƠ MỚI NHẤT (dựa vào CreatedAt)
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
                    return NotFound(new { success = false, message = "Không tìm thấy hồ sơ kiểm định nào" });
                }

                // Lấy thông tin Vehicle
                var vehicle = await _context.Vehicles.FindAsync(latestInspection.VehicleId);

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        latestInspection.InspectionId,
                        latestInspection.InspectionCode,
                        latestInspection.InspectionType,
                        latestInspection.Status,
                        latestInspection.CreatedAt,
                        latestInspection.Count_Re,
                        latestInspection.Notes,
                        VehiclePlate = vehicle?.PlateNo,
                        latestInspection.OwnerId
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
        /// LẤY LỊCH SỬ KIỂM ĐỊNH CỦA XE (5 lượt gần nhất - dựa vào CreatedAt)
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
    }

    // DTO for Approve Inspection Request
    public class ApproveInspectionRequest
    {
        public int VehicleId { get; set; }
        public Guid OwnerId { get; set; }
        public string InspectionType { get; set; }
        public string? InspectionCode { get; set; } // Tùy chọn, nếu không có sẽ tự sinh
        public string? Notes { get; set; }
    }
}