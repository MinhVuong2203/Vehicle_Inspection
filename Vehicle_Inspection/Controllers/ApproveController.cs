using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;  // ✅ BẮT BUỘC - Cung cấp SaveChangesAsync() và Database
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
        /// TỰ ĐỘNG PHÁT HIỆN LOẠI KIỂM ĐỊNH
        /// Logic:
        /// - Nếu chưa có lịch sử → FIRST
        /// - Nếu có hồ sơ Status = 7 (Đã cấp GCN) → PERIODIC
        /// - Nếu có hồ sơ Status = 6 (Không đạt) → RE_INSPECTION
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

                // Lấy lịch sử kiểm định của xe (sắp xếp theo thời gian mới nhất)
                var history = await _context.Inspections
                    .Where(i => i.VehicleId == vehicleId && i.IsDeleted == false)
                    .OrderByDescending(i => i.CreatedAt)
                    .Select(i => new
                    {
                        i.InspectionId,
                        i.InspectionCode,
                        i.InspectionType,
                        i.Status,
                        i.CreatedAt
                    })
                    .Take(5) // Lấy 5 lượt gần nhất
                    .ToListAsync();

                Console.WriteLine($"📊 Tìm thấy {history.Count} lượt kiểm định trong lịch sử");

                // Debug: In ra chi tiết từng lượt
                foreach (var item in history)
                {
                    Console.WriteLine($"   - ID: {item.InspectionId}, Code: {item.InspectionCode}, Status: {item.Status}, Type: {item.InspectionType}");
                }

                string detectedType = "FIRST";
                string reason = "";

                if (history == null || history.Count == 0)
                {
                    // ✅ TRƯỜNG HỢP 1: Chưa có lịch sử → FIRST
                    detectedType = "FIRST";
                    reason = "Xe chưa từng kiểm định trước đó";
                    Console.WriteLine("✅ Kết quả: FIRST (Chưa có lịch sử)");
                }
                else
                {
                    // Lấy lượt kiểm định gần nhất
                    var latestInspection = history.First();
                    Console.WriteLine($"🔍 Lượt gần nhất: Code={latestInspection.InspectionCode}, Status={latestInspection.Status}");

                    if (latestInspection.Status == 6)
                    {
                        // ✅ TRƯỜNG HỢP 2: Lượt gần nhất không đạt → RE_INSPECTION
                        detectedType = "RE_INSPECTION";
                        reason = $"Lượt kiểm định gần nhất ({latestInspection.InspectionCode}) không đạt, cần tái kiểm";
                        Console.WriteLine($"✅ Kết quả: RE_INSPECTION (Status = 6)");
                    }
                    else if (latestInspection.Status == 7)
                    {
                        // ✅ TRƯỜNG HỢP 3: Lượt gần nhất đã cấp GCN → PERIODIC
                        detectedType = "PERIODIC";
                        reason = $"Xe đã có giấy chứng nhận ({latestInspection.InspectionCode}), thực hiện kiểm định định kỳ";
                        Console.WriteLine($"✅ Kết quả: PERIODIC (Status = 7)");
                    }
                    else if (latestInspection.Status == 5)
                    {
                        // ✅ TRƯỜNG HỢP 3.5: Lượt gần nhất đạt (Status = 5) → PERIODIC
                        detectedType = "PERIODIC";
                        reason = $"Lượt kiểm định trước đã đạt ({latestInspection.InspectionCode}), thực hiện kiểm định định kỳ";
                        Console.WriteLine($"✅ Kết quả: PERIODIC (Status = 5 - Đạt)");
                    }
                    else
                    {
                        // TRƯỜNG HỢP ĐẶC BIỆT: Status khác (0,1,2,3,4,8)
                        Console.WriteLine($"⚠️ Status đặc biệt: {latestInspection.Status}, tìm lượt hoàn thành gần nhất...");

                        // Tìm lượt có Status = 5, 6 hoặc 7 gần nhất
                        var completedInspection = history.FirstOrDefault(h => h.Status == 5 || h.Status == 6 || h.Status == 7);

                        if (completedInspection != null)
                        {
                            Console.WriteLine($"🔍 Tìm thấy lượt hoàn thành: Code={completedInspection.InspectionCode}, Status={completedInspection.Status}");

                            if (completedInspection.Status == 7 || completedInspection.Status == 5)
                            {
                                detectedType = "PERIODIC";
                                reason = $"Xe đã có lượt kiểm định đạt trước đó, thực hiện kiểm định định kỳ";
                                Console.WriteLine($"✅ Kết quả: PERIODIC (Status = {completedInspection.Status})");
                            }
                            else if (completedInspection.Status == 6)
                            {
                                detectedType = "RE_INSPECTION";
                                reason = $"Lượt kiểm định trước đó không đạt, cần tái kiểm";
                                Console.WriteLine($"✅ Kết quả: RE_INSPECTION (Status = 6)");
                            }
                        }
                        else
                        {
                            // Nếu không có lượt nào hoàn thành → Coi như FIRST
                            detectedType = "FIRST";
                            reason = "Chưa có lượt kiểm định hoàn thành, coi như lần đầu";
                            Console.WriteLine($"✅ Kết quả: FIRST (Không có lượt hoàn thành)");
                        }
                    }
                }

                Console.WriteLine($"✅ Kết luận cuối cùng: {detectedType}");
                Console.WriteLine($"📝 Lý do: {reason}");

                return Ok(new
                {
                    success = true,
                    data = new
                    {
                        inspectionType = detectedType,
                        reason = reason,
                        history = history
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
        /// Tạo Inspection mới (không cần LaneId)
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateInspection([FromBody] CreateInspectionRequest request)
        {
            // ✅ SỬA: Dùng ExecutionStrategy thay vì transaction thủ công
            try
            {
                Console.WriteLine("💾 ========== BẮT ĐẦU TẠO INSPECTION ==========");
                Console.WriteLine($"📋 InspectionCode: {request.InspectionCode}");
                Console.WriteLine($"📋 VehicleId: {request.VehicleId}");
                Console.WriteLine($"📋 OwnerId: {request.OwnerId}");
                Console.WriteLine($"📋 InspectionType: {request.InspectionType}");

                // Validate dữ liệu
                if (string.IsNullOrWhiteSpace(request.InspectionCode))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã lượt kiểm định không được để trống"
                    });
                }

                if (request.VehicleId <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "VehicleId không hợp lệ"
                    });
                }

                if (request.OwnerId == Guid.Empty)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "OwnerId không hợp lệ"
                    });
                }

                if (string.IsNullOrWhiteSpace(request.InspectionType))
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Loại kiểm định không hợp lệ"
                    });
                }

                // Kiểm tra InspectionCode đã tồn tại chưa
                var existingCode = await _context.Inspections
                    .FirstOrDefaultAsync(i => i.InspectionCode == request.InspectionCode);

                if (existingCode != null)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Mã lượt kiểm định đã tồn tại"
                    });
                }

                // Kiểm tra Vehicle có tồn tại không
                var vehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.VehicleId == request.VehicleId);

                if (vehicle == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy thông tin phương tiện"
                    });
                }

                // Kiểm tra Owner có tồn tại không
                var owner = await _context.Owners
                    .FirstOrDefaultAsync(o => o.OwnerId == request.OwnerId);

                if (owner == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Không tìm thấy thông tin chủ xe"
                    });
                }

                // Tạo Inspection mới (không có LaneId)
                var inspection = new Inspection
                {
                    InspectionCode = request.InspectionCode,
                    VehicleId = request.VehicleId,
                    OwnerId = request.OwnerId,
                    InspectionType = request.InspectionType,
                    LaneId = null, // Không gán dây chuyền lúc tạo
                    Status = 1, // RECEIVED
                    CreatedAt = DateTime.Now,
                    Notes = request.Notes,
                    IsDeleted = false
                };

                _context.Inspections.Add(inspection);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Inspection created with ID: {inspection.InspectionId}");
                Console.WriteLine("✅ Save successful");

                return Ok(new
                {
                    success = true,
                    message = "Xét duyệt thành công",
                    data = new
                    {
                        inspectionId = inspection.InspectionId,
                        inspectionCode = inspection.InspectionCode,
                        inspectionType = inspection.InspectionType
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
                    message = "Có lỗi xảy ra khi xét duyệt",
                    error = ex.Message
                });
            }
        }
    }

    // DTO for Create Inspection Request (không có LaneId)
    public class CreateInspectionRequest
    {
        public string InspectionCode { get; set; }
        public int VehicleId { get; set; }
        public Guid OwnerId { get; set; }
        public string InspectionType { get; set; }
        public string? Notes { get; set; }
    }
}