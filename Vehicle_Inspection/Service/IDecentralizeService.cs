using Vehicle_Inspection.Models;

namespace Vehicle_Inspection.Service
{
    public interface IDecentralizeService
    {
        Task<List<User>> GetFilteredUsersAsync(string search, int? position, int? team, string gender, string sort);
        Task<List<Role>> GetAllRolesAsync();
        Task<List<Position>> GetAllPositionsAsync();
        Task<List<Team>> GetAllTeamsAsync();
        Task<List<Stage>> GetAllStagesAsync();
        Task<bool> UpdateUserRoleAsync(Guid userId, int roleId, bool isChecked);
        Task<bool> UpdateUserStageAsync(Guid userId, int stageId, bool isChecked);
    }
}