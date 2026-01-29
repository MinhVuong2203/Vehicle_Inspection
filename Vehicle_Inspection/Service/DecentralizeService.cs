using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vehicle_Inspection.Data;
using Vehicle_Inspection.Models;

namespace Vehicle_Inspection.Service
{
    public class DecentralizeService : IDecentralizeService
    {
        private readonly VehInsContext _context;

        public DecentralizeService(VehInsContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetFilteredUsersAsync(string search, int? position, int? team, string gender, string sort)
        {
            var query = _context.Users
                .Include(u => u.Position)
                .Include(u => u.Team)
                .Include(u => u.Roles)
                .Include(u => u.Stages)
                .Where(u => u.IsActive)
                .AsQueryable();

            // Apply filters
            if (!string.IsNullOrEmpty(search))
            {
                search = search.Trim().ToLower();
                query = query.Where(u =>
                    u.FullName.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search) ||
                    u.Phone.Contains(search));
            }

            if (position.HasValue)
            {
                query = query.Where(u => u.PositionId == position);
            }

            if (team.HasValue)
            {
                query = query.Where(u => u.TeamId == team);
            }

            if (!string.IsNullOrEmpty(gender))
            {
                query = query.Where(u => u.Gender == gender);
            }

            // Apply sorting
            query = sort switch
            {
                "FullName" => query.OrderBy(u => u.FullName),
                "FullName_desc" => query.OrderByDescending(u => u.FullName),
                "BirthDate" => query.OrderBy(u => u.BirthDate),
                "BirthDate_desc" => query.OrderByDescending(u => u.BirthDate),
                "CreatedAt" => query.OrderBy(u => u.CreatedAt),
                "CreatedAt_desc" => query.OrderByDescending(u => u.CreatedAt),
                _ => query.OrderByDescending(u => u.CreatedAt)
            };

            return await query.ToListAsync();
        }

        public async Task<List<Role>> GetAllRolesAsync()
        {
            return await _context.Roles.OrderBy(r => r.RoleName).ToListAsync();
        }

        public async Task<List<Position>> GetAllPositionsAsync()
        {
            return await _context.Positions.OrderBy(p => p.PositionName).ToListAsync();
        }

        public async Task<List<Team>> GetAllTeamsAsync()
        {
            return await _context.Teams.OrderBy(t => t.TeamName).ToListAsync();
        }

        public async Task<List<Stage>> GetAllStagesAsync()
        {
            return await _context.Stages
                .Where(s => s.IsActive == true)
                .OrderBy(s => s.StageName)
                .ToListAsync();
        }

        public async Task<bool> UpdateUserRoleAsync(Guid userId, int roleId, bool isChecked)
        {
            try
            {
                // Load user with roles
                var user = await _context.Users
                    .Include(u => u.Roles)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    return false;
                }

                // Load role
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);
                if (role == null)
                {
                    return false;
                }

                // Check if user already has this role
                var existingRole = user.Roles.FirstOrDefault(r => r.RoleId == roleId);

                if (isChecked)
                {
                    // Add role if not exists
                    if (existingRole == null)
                    {
                        user.Roles.Add(role);
                    }
                }
                else
                {
                    // Remove role if exists
                    if (existingRole != null)
                    {
                        user.Roles.Remove(existingRole);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user role: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> UpdateUserStageAsync(Guid userId, int stageId, bool isChecked)
        {
            try
            {
              
                var user = await _context.Users
                    .Include(u => u.Stages)
                    .Include(u => u.Position)
                    .FirstOrDefaultAsync(u => u.UserId == userId);

                if (user == null)
                {
                    return false;
                }

                if (user.Position?.PoitionCode != "KTV")
                {
                    Console.WriteLine($"User {userId} is not KTV, cannot assign stages");
                    return false;
                }

                var stage = await _context.Stages.FirstOrDefaultAsync(s => s.StageId == stageId);
                if (stage == null)
                {
                    return false;
                }

                var existingStage = user.Stages.FirstOrDefault(s => s.StageId == stageId);

                if (isChecked)
                {        
                    if (existingStage == null)
                    {
                        user.Stages.Add(stage);
                    }
                }
                else
                {
                    if (existingStage != null)
                    {
                        user.Stages.Remove(existingStage);
                    }
                }

                await _context.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error updating user stage: {ex.Message}");
                return false;
            }
        }
    }
}