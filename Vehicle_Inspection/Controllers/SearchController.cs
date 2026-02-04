using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Vehicle_Inspection.Data;

namespace Vehicle_Inspection.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SearchController : ControllerBase
    {
        private readonly VehInsContext _context;

        public SearchController(VehInsContext context)
        {
            _context = context;
        }

        [HttpGet("global")]
        public async Task<IActionResult> GlobalSearch([FromQuery] string q)
        {
            if (string.IsNullOrWhiteSpace(q) || q.Length < 2)
            {
                return Ok(new { results = new List<object>() });
            }

            var query = q.Trim().ToLower();
            var results = new List<object>();

            // 1. Tìm Inspection theo mã
            var inspections = await _context.Inspections
                .Where(i => !i.IsDeleted && i.InspectionCode.ToLower().Contains(query))
                .Take(5)
                .Select(i => new
                {
                    type = "inspection",
                    id = i.InspectionId,
                    code = i.InspectionCode,
                    title = i.InspectionCode,
                    subtitle = $"{i.InspectionType} - {i.Vehicle.PlateNo}",
                    url = $"/Inspection/Details/{i.InspectionCode}",
                    icon = "fa-clipboard-check"
                })
                .ToListAsync();

            results.AddRange(inspections);

            // 2. Tìm Vehicle theo biển số
            var vehicles = await _context.Vehicles
                .Where(v => v.PlateNo.ToLower().Contains(query))
                .Take(5)
                .Select(v => new
                {
                    type = "vehicle",
                    id = v.VehicleId,
                    code = v.PlateNo,
                    title = v.PlateNo,
                    subtitle = $"{v.VehicleType.TypeName} - {v.Owner.FullName}",
                    url = $"/Vehicle/Details/{v.VehicleId}",
                    icon = "fa-car"
                })
                .ToListAsync();

            results.AddRange(vehicles);

            // 3. Tìm Employee theo tên
            var employees = await _context.Users
                .Where(u => u.IsActive && u.FullName.ToLower().Contains(query))
                .Take(5)
                .Select(u => new
                {
                    type = "employee",
                    id = u.UserId,
                    code = u.CCCD,
                    title = u.FullName,
                    subtitle = $"{u.Position.PositionName} - {u.Phone}",
                    url = $"/Employee/Details/{u.UserId}",
                    icon = "fa-user"
                })
                .ToListAsync();

            results.AddRange(employees);

            return Ok(new { results });
        }
    }
}