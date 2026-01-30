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
        /// - Nếu có hồ sơ Status = 7 (Đã cấp GCN) hoặc Status = 5 (Đạt) → PERIODIC
        /// - Nếu có hồ sơ Status = 6 (Không đạt) → RE_INSPECTION
        /// </summary>
        [HttpGet("detect-type")]
        public async Task<IActionResult> DetectInspectionType([FromQuery] int vehicleId)
        {
            try
            {
                Console.WriteLine($"🔍 ========== PHÁT HIỆN LOẠI KIỂM ĐỊNH ==========");
                Console.WriteLine($"📋 VehicleId: {vehicleId}");
                Console.WriteLine($"📋 Current DateTime: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");

                if (vehicleId <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "VehicleId không hợp lệ"
                    });
                }

                // 🔧 FIX: Thêm xử lý IsDeleted có thể null
                // Lấy lịch sử kiểm định của xe (sắp xếp theo thời gian mới nhất)
                var history = await _context.Inspections
                    .Where(i => i.VehicleId == vehicleId && (i.IsDeleted == false || i.IsDeleted == null))
                    .OrderByDescending(i => i.CreatedAt)
                    .Select(i => new
                    {
                        i.InspectionId,
                        i.InspectionCode,
                        i.InspectionType,
                        i.Status,
                        i.CreatedAt,
                        i.IsDeleted
                    })
                    .Take(10) // 🔧 FIX: Tăng lên 10 lượt để phân tích kỹ hơn
                    .ToListAsync();

                Console.WriteLine($"📊 Tìm thấy {history.Count} lượt kiểm định trong lịch sử");

                // 🔧 FIX: Debug chi tiết hơn - In ra tất cả records
                if (history.Count > 0)
                {
                    Console.WriteLine("📋 CHI TIẾT LỊCH SỬ:");
                    foreach (var item in history)
                    {
                        Console.WriteLine($"   - ID: {item.InspectionId}, Code: {item.InspectionCode}, Status: {item.Status}, Type: {item.InspectionType}, Date: {item.CreatedAt:yyyy-MM-dd}, IsDeleted: {item.IsDeleted}");
                    }
                }
                else
                {
                    Console.WriteLine("⚠️ KHÔNG CÓ LỊCH SỬ KIỂM ĐỊNH");
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
                    Console.WriteLine($"🔍 Lượt gần nhất: Code={latestInspection.InspectionCode}, Status={latestInspection.Status}, Type={latestInspection.InspectionType}");

                    // 🔧 FIX: Kiểm tra Status với switch-case rõ ràng hơn
                    switch (latestInspection.Status)
                    {
                        case 6: // Không đạt
                            detectedType = "RE_INSPECTION";
                            reason = $"Lượt kiểm định gần nhất ({latestInspection.InspectionCode}) không đạt, cần tái kiểm";
                            Console.WriteLine($"✅ Kết quả: RE_INSPECTION (Status = 6 - Không đạt)");
                            break;

                        case 7: // Đã cấp GCN
                            detectedType = "PERIODIC";
                            reason = $"Xe đã có giấy chứng nhận ({latestInspection.InspectionCode}), thực hiện kiểm định định kỳ";
                            Console.WriteLine($"✅ Kết quả: PERIODIC (Status = 7 - Đã cấp GCN)");
                            break;

                        case 5: // Đạt
                            detectedType = "PERIODIC";
                            reason = $"Lượt kiểm định trước đã đạt ({latestInspection.InspectionCode}), thực hiện kiểm định định kỳ";
                            Console.WriteLine($"✅ Kết quả: PERIODIC (Status = 5 - Đạt)");
                            break;

                        default: // Status 0,1,2,3,4,8
                            Console.WriteLine($"⚠️ Status đặc biệt: {latestInspection.Status}, tìm lượt hoàn thành gần nhất...");

                            // Tìm lượt có Status = 5, 6 hoặc 7 gần nhất (đã hoàn thành)
                            var completedInspection = history.FirstOrDefault(h => h.Status == 5 || h.Status == 6 || h.Status == 7);

                            if (completedInspection != null)
                            {
                                Console.WriteLine($"🔍 Tìm thấy lượt hoàn thành: Code={completedInspection.InspectionCode}, Status={completedInspection.Status}");

                                if (completedInspection.Status == 7 || completedInspection.Status == 5)
                                {
                                    detectedType = "PERIODIC";
                                    reason = $"Xe đã có lượt kiểm định đạt trước đó ({completedInspection.InspectionCode}), thực hiện kiểm định định kỳ";
                                    Console.WriteLine($"✅ Kết quả: PERIODIC (Tìm thấy Status = {completedInspection.Status})");
                                }
                                else if (completedInspection.Status == 6)
                                {
                                    detectedType = "RE_INSPECTION";
                                    reason = $"Lượt kiểm định trước đó ({completedInspection.InspectionCode}) không đạt, cần tái kiểm";
                                    Console.WriteLine($"✅ Kết quả: RE_INSPECTION (Tìm thấy Status = 6)");
                                }
                            }
                            else
                            {
                                // Nếu không có lượt nào hoàn thành → Coi như FIRST
                                detectedType = "FIRST";
                                reason = "Chưa có lượt kiểm định hoàn thành, coi như lần đầu";
                                Console.WriteLine($"✅ Kết quả: FIRST (Không có lượt hoàn thành)");
                            }
                            break;
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
                        history = history.Select(h => new
                        {
                            h.InspectionId,
                            h.InspectionCode,
                            h.InspectionType,
                            h.Status,
                            StatusText = GetStatusText(h.Status),
                            h.CreatedAt
                        })
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error: {ex.Message}");
                Console.WriteLine($"❌ StackTrace: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi phát hiện loại kiểm định",
                    error = ex.Message,
                    stackTrace = ex.StackTrace
                });
            }
        }

        /// <summary>
        /// Helper: Lấy text mô tả status
        /// </summary>
        private string GetStatusText(int? status)
        {
            return status switch
            {
                0 => "Nháp",
                1 => "Đã tiếp nhận",
                2 => "Đã thu phí",
                3 => "Đang kiểm định",
                4 => "Hoàn thành KĐ",
                5 => "Đạt",
                6 => "Không đạt",
                7 => "Đã cấp GCN",
                8 => "Đã hủy",
                _ => "N/A"
            };
        }

        /// <summary>
        /// Tạo Inspection mới (không cần LaneId)
        /// </summary>
        [HttpPost("create")]
        public async Task<IActionResult> CreateInspection([FromBody] CreateInspectionRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

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

                await transaction.CommitAsync();
                Console.WriteLine("✅ Transaction committed successfully");

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

                await transaction.RollbackAsync();

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