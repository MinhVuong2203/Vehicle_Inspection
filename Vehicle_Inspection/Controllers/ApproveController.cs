using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vehicle_Inspection.Data;
using Vehicle_Inspection.Models;

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
        /// Lấy danh sách dây chuyền (Lanes)
        /// </summary>
        [HttpGet("lanes")]
        public async Task<IActionResult> GetLanes()
        {
            try
            {
                var lanes = await _context.Lanes
                    .Where(l => l.IsActive == true)
                    .OrderBy(l => l.LaneCode)
                    .Select(l => new
                    {
                        l.LaneId,
                        l.LaneCode,
                        l.LaneName
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    data = lanes
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Có lỗi xảy ra khi tải danh sách dây chuyền",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// Xét duyệt và tạo Inspection mới
        /// </summary>
        [HttpPost("approve")]
        public async Task<IActionResult> ApproveInspection([FromBody] InspectionApprovalRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                Console.WriteLine("💾 ========== BẮT ĐẦU TẠO INSPECTION ==========");
                Console.WriteLine($"📋 InspectionCode: {request.InspectionCode}");
                Console.WriteLine($"📋 VehicleId: {request.VehicleId}");
                Console.WriteLine($"📋 OwnerId: {request.OwnerId}");
                Console.WriteLine($"📋 InspectionType: {request.InspectionType}");
                Console.WriteLine($"📋 LaneId: {request.LaneId}");

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
                        message = "Vui lòng chọn loại kiểm định"
                    });
                }

                if (!request.LaneId.HasValue || request.LaneId <= 0)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "Vui lòng chọn dây chuyền"
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

                // Kiểm tra Lane có tồn tại không
                var lane = await _context.Lanes
                    .FirstOrDefaultAsync(l => l.LaneId == request.LaneId && l.IsActive == true);

                if (lane == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "Dây chuyền không tồn tại hoặc không hoạt động"
                    });
                }

                // Tạo Inspection mới
                var inspection = new Inspection
                {
                    InspectionCode = request.InspectionCode,
                    VehicleId = request.VehicleId,
                    OwnerId = request.OwnerId,
                    InspectionType = request.InspectionType,
                    LaneId = request.LaneId,
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
                        inspectionCode = inspection.InspectionCode
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

    // DTO for Inspection Approval Request
    public class InspectionApprovalRequest
    {
        public string InspectionCode { get; set; }
        public int VehicleId { get; set; }
        public Guid OwnerId { get; set; }
        public string InspectionType { get; set; }
        public int? LaneId { get; set; }
        public string? Notes { get; set; }
    }
}