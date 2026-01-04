using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Vehicle_Inspection.Models;

namespace Vehicle_Inspection.Service
{
    public interface IEmployee
    {
        Task<List<User>> GetAllEmployeesAsync();
        Task<User> GetEmployeeByIdAsync(Guid id);
        Task CreateEmployeeAsync(User employee);
        Task UpdateEmployeeAsync(User employee);
    }
}
