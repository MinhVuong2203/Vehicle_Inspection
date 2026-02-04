using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Vehicle_Inspection.Data;
using Vehicle_Inspection.Service;

namespace Vehicle_Inspection.Controllers
{
    public class DecentralizeController : Controller
    {
        private readonly IDecentralizeService _decentralizeService;
        private readonly VehInsContext _context;

        public DecentralizeController(IDecentralizeService decentralizeService, VehInsContext context)
        {
            _decentralizeService = decentralizeService;
            _context = context;
        }

        public async Task<IActionResult> Index(string search, int? position, int? team, string gender, bool? isActive, string sort, string mode = "role", int page = 1)
        {

            Guid userId = Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value);
            var currentUser = _context.Users
                .Include(u => u.Position)
                .FirstOrDefault(u => u.UserId == userId);

            var positionCode = currentUser?.Position?.PoitionCode;
            var currentTeamId = currentUser?.TeamId;
            ViewBag.PositionCode = positionCode;
            if (positionCode == "TTDC")
            {
                mode = "stage";
            }

            int pageSize = 10; // Số nhân viên mỗi trang

            var employees = await _decentralizeService.GetFilteredUsersAsync(search, position, team, gender, sort);

            if (positionCode == "TTDC")
            {
                employees = employees
                    .Where(u => u.TeamId == currentTeamId)
                    .ToList();
            }



            var roles = await _decentralizeService.GetAllRolesAsync();
            var stages = await _decentralizeService.GetAllStagesAsync();

            var allViewModel = employees.Select(user => new
            {
                User = user,
                RoleAssignments = roles.ToDictionary(
                    role => role.RoleId,
                    role => user.Roles.Any(r => r.RoleId == role.RoleId)
                ),
                StageAssignments = stages.ToDictionary(
                    stage => stage.StageId,
                    stage => user.Stages.Any(s => s.StageId == stage.StageId)
                )
            }).ToList();

            // Đếm tổng số
            var totalItems = allViewModel.Count;

            // Phân trang
            var paginatedViewModel = allViewModel
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();




           


            // Truyền thông tin phân trang
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
            ViewBag.TotalItems = totalItems;

            ViewBag.Positions = await _decentralizeService.GetAllPositionsAsync();
         
            ViewBag.Roles = roles;
            ViewBag.Stages = stages;
            ViewBag.Mode = mode;


            var teams = await _decentralizeService.GetAllTeamsAsync();

            if (positionCode == "TTDC")
            {
                teams = teams
                    .Where(t => t.TeamId == currentTeamId)
                    .ToList();
            }

            ViewBag.Teams = teams;


            return View(paginatedViewModel);
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserRole([FromBody] UpdateUserRoleRequest request)
        {
            try
            {
                if (request == null || request.UserId == Guid.Empty || request.RoleId <= 0)
                {
                    return BadRequest(new { message = "Invalid request data." });
                }

                var success = await _decentralizeService.UpdateUserRoleAsync(request.UserId, request.RoleId, request.IsChecked);

                if (!success)
                {
                    return NotFound(new { message = "User or Role not found." });
                }

                return Ok(new { message = "Role updated successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> UpdateUserStage([FromBody] UpdateUserStageRequest request)
        {
            try
            {
                if (request == null || request.UserId == Guid.Empty || request.StageId <= 0)
                {
                    return BadRequest(new { message = "Invalid request data." });
                }

                var success = await _decentralizeService.UpdateUserStageAsync(request.UserId, request.StageId, request.IsChecked);

                if (!success)
                {
                    return NotFound(new { message = "User or Stage not found." });
                }

                return Ok(new { message = "Stage updated successfully." });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred.", error = ex.Message });
            }
        }
    }

    // Request model for UpdateUserRole
    public class UpdateUserRoleRequest
    {
        public Guid UserId { get; set; }
        public int RoleId { get; set; }
        public bool IsChecked { get; set; }
    }

    // Request model for UpdateUserStage
    public class UpdateUserStageRequest
    {
        public Guid UserId { get; set; }
        public int StageId { get; set; }
        public bool IsChecked { get; set; }
    }
}