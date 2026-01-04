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
            return await _context.Users.Include(u => u.Position).Include(u => u.Team).ToListAsync();
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
            _context.Users.Add(employee);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateEmployeeAsync(User employee)
        {
            _context.Users.Update(employee);
            await _context.SaveChangesAsync();
        }
    }
}
