using Microsoft.EntityFrameworkCore;
using Vehicle_Inspection.Data;
using Vehicle_Inspection.Models;

namespace Vehicle_Inspection.Service
{
    public class TollService : ITollService
    {
        private readonly VehInsContext _context;

        public TollService(VehInsContext context)
        {
            _context = context;
        }

        public List<object> GetInspections(string? search, int? status)
        {
            return _context.Inspections
                .Where(i => !i.IsDeleted && (status == null || i.Status == status) &&
                            (string.IsNullOrEmpty(search) || i.InspectionCode.Contains(search) || i.InspectionType.Contains(search)))
                .Select(i => new
                {
                    i.InspectionCode,
                    i.InspectionType,
                    i.Status
                })
                .ToList<object>();
        }
    }
}
