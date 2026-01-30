using Microsoft.EntityFrameworkCore;
using Vehicle_Inspection.Data;
using Vehicle_Inspection.Models;

namespace Vehicle_Inspection.Service
{
    public class CertificatesService : ICertificatesService
    {
        private readonly VehInsContext _context;

        public CertificatesService(VehInsContext context)
        {
            _context = context;
        }

        public async Task<List<Inspection>> GetCompletedInspectionsAsync()
        {
            return await _context.Inspections
                .Include(i => i.Vehicle)
                    .ThenInclude(v => v.Owner)
                .Include(i => i.Vehicle.VehicleType)
                .Include(i => i.Lane)
                .Include(i => i.CreatedByNavigation)
                .Include(i => i.ConcludedByNavigation)
                .Include(i => i.ReceivedByNavigation)
                .Where(i => i.Status == 5 && !i.IsDeleted)
                .OrderByDescending(i => i.CreatedAt)
                .ToListAsync();
        }

        public async Task<Inspection?> GetInspectionForCertificateAsync(int inspectionId)
        {
            return await _context.Inspections
                .Include(i => i.Vehicle)
                    .ThenInclude(v => v.Owner)
                .Include(i => i.Vehicle.VehicleType)
                .Include(i => i.Vehicle.Specification)
                .Include(i => i.Certificate)
                .Include(i => i.Lane)
                .Where(i => i.InspectionId == inspectionId && i.Status == 5 && !i.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public int CalculateValidityMonths(Vehicle vehicle)
        {
            if (vehicle == null || vehicle.ManufactureYear == null)
                return 12;

            int vehicleAge = DateTime.Now.Year - vehicle.ManufactureYear.Value;
            bool hasCommercialUse = vehicle.HasCommercialModification ?? false;
            bool hasModification = vehicle.HasModification ?? false;

            // Lấy số chỗ ngồi từ Specification
            int? seatingCapacity = vehicle.Specification?.SeatingCapacity;

            // 1. Ô tô chở người đến 08 chỗ (không kể chỗ của người lái xe) không kinh doanh vận tải
            if (!hasCommercialUse && seatingCapacity <= 8)
            {
                if (vehicleAge <= 7) return 24;
                if (vehicleAge <= 20) return 12;
                return 6;
            }
            // 2. Ô tô chở người các loại đến 08 chỗ (không kể chỗ của người lái xe) có kinh doanh vận tải
            else if (hasCommercialUse && seatingCapacity <= 8)
            {
                if (vehicleAge <= 5) return 12;
                if (vehicleAge > 5 && !hasModification) return 6;
                if (hasModification) return 6; // Có cải tạo chu kỳ 12 hoặc 6 tháng
                return 6;
            }
            // 3. Ô tô chở người các loại trên 08 chỗ (không kể chỗ của người lái xe) và ô tô chở người chuyên dùng
            else if (seatingCapacity > 8)
            {
                if (vehicleAge <= 5) return 12;
                if (vehicleAge > 5 && !hasModification) return 6;
                if (hasModification) return 6; // Có cải tạo

                // Xe trên 15 năm, đã cải tạo thành 6-8 chỗ
                if (vehicleAge >= 15 && hasModification && seatingCapacity <= 8) return 3;

                return 6;
            }
            // 4. Ô tô tải các loại, ô tô chuyên dùng, ô tô đầu kéo, rơ moóc, sơmi rơ moóc
            else
            {
                if (vehicleAge <= 7) return 12;
                return 12; // Mặc định 12 tháng
            }
        }
    }
}