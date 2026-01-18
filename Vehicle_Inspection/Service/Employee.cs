using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Vehicle_Inspection.Data;
using Vehicle_Inspection.Models;

namespace Vehicle_Inspection.Service
{
    public class Employee : IEmployee
    {
        private readonly VehInsContext _context;

        public Employee(VehInsContext context)
        {
            _context = context;
        }

        public async Task<List<User>> GetAllEmployeesAsync()
        {
            return await _context.Users.Where(u => u.IsActive).Include(u => u.Position).Include(u => u.Team).ToListAsync();
        }

        public async Task<User> GetEmployeeByIdAsync(Guid id)
        {
            var employee = await _context.Users
                .Include(u => u.Account)  // QUAN TRỌNG: Include Account
                .Include(u => u.Position)
                .Include(u => u.Team)
                .FirstOrDefaultAsync(u => u.UserId == id);
            return employee;
        }

        public async Task CreateEmployeeAsync(User employee)
        {
            // đảm bảo FK account
            if (employee.Account != null)
                employee.Account.UserId = employee.UserId;

            _context.Users.Add(employee);
            await _context.SaveChangesAsync();
        }


        public async Task UpdateEmployeeAsync(User model)
        {
            var entity = await _context.Users
                .Include(u => u.Account)
                .FirstOrDefaultAsync(u => u.UserId == model.UserId);

            if (entity == null) throw new Exception("Employee not found");

            entity.FullName = model.FullName;
            entity.Phone = model.Phone;
            entity.Email = model.Email;
            entity.BirthDate = model.BirthDate;
            entity.CCCD = model.CCCD;
            entity.Gender = model.Gender;
            entity.Level = model.Level;
            entity.PositionId = model.PositionId;
            entity.TeamId = model.TeamId;

            // Address split columns
            entity.Address = model.Address;
            entity.Ward = model.Ward;
            entity.Province = model.Province;

            if (!string.IsNullOrWhiteSpace(model.ImageUrl))
                entity.ImageUrl = model.ImageUrl;

            if (model.Account != null)
            {
                entity.Account ??= new Account { UserId = entity.UserId };

                entity.Account.Username = string.IsNullOrWhiteSpace(model.Account.Username)
                    ? null
                    : model.Account.Username.Trim();

                if (!string.IsNullOrWhiteSpace(model.Account.PasswordHash))
                {
                    entity.Account.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.Account.PasswordHash);
                }
            }

            await _context.SaveChangesAsync();
        }

        public async Task DeleteSoftAsync(Guid id)
        {
            var entity = await _context.Users.FindAsync(id);
            if (entity == null) throw new Exception("Nhân viên không tồn tại");
            entity.IsActive = false;         
            await _context.SaveChangesAsync();
        }

        public async Task RestoreAsync(Guid id)
        {
            var entity = await _context.Users.FindAsync(id);
            if (entity == null) throw new Exception("Cán bộ không tồn tại");
            entity.IsActive = true;
            await _context.SaveChangesAsync();
        }
    }
}
