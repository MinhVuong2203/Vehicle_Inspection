using Microsoft.AspNetCore.Mvc;
using Vehicle_Inspection.Service;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Vehicle_Inspection.Controllers
{
    public class DecentralizeController : Controller
    {
        private readonly IDecentralizeService _decentralizeService;

        public DecentralizeController(IDecentralizeService decentralizeService)
        {
            _decentralizeService = decentralizeService;
        }

        public async Task<IActionResult> Index(string search, int? position, int? team, string gender, bool? isActive, string sort, string mode = "role")
        {
            var employees = await _decentralizeService.GetFilteredUsersAsync(search, position, team, gender, sort);
            var roles = await _decentralizeService.GetAllRolesAsync();
            var stages = await _decentralizeService.GetAllStagesAsync();

            var viewModel = employees.Select(user => new
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

            ViewBag.Positions = await _decentralizeService.GetAllPositionsAsync();
            ViewBag.Teams = await _decentralizeService.GetAllTeamsAsync();
            ViewBag.Roles = roles;
            ViewBag.Stages = stages;
            ViewBag.Mode = mode; // "role" or "stage"

            return View(viewModel);
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