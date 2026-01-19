using Microsoft.EntityFrameworkCore;
using Vehicle_Inspection.Data;

namespace Vehicle_Inspection.Service
{
    public class InspectionService : IInspectionService
    {
        private readonly VehInsContext _context;

        public InspectionService(VehInsContext context)
        {
            _context = context;
        }
        //lấy danh sách hồ sơ kiểm định
        public List<InspectionRecordDto> GetInspectionRecords()
        {
            try
            {
                var records = _context.Inspections
                    .Where(i => !i.IsDeleted)
                    .Include(i => i.Vehicle)
                    .Include(i => i.Owner)
                    .Include(i => i.Lane)
                    .OrderByDescending(i => i.CreatedAt)
                    .Select(i => new InspectionRecordDto
                    {
                        // Thông tin hồ sơ kiểm định
                        InspectionId = i.InspectionId,
                        InspectionCode = i.InspectionCode,
                        InspectionType = i.InspectionType,
                        Status = i.Status,
                        FinalResult = i.FinalResult,
                        CreatedAt = i.CreatedAt,
                        ReceivedAt = i.ReceivedAt,
                        PaidAt = i.PaidAt,
                        CompletedAt = i.CompletedAt,

                        // Thông tin xe
                        PlateNo = i.Vehicle.PlateNo,
                        InspectionNo = i.Vehicle.InspectionNo,
                        VehicleGroup = i.Vehicle.VehicleGroup,
                        VehicleType = i.Vehicle.VehicleType.TypeName,
                        Brand = i.Vehicle.Brand,
                        Model = i.Vehicle.Model,
                        EngineNo = i.Vehicle.EngineNo,
                        Chassis = i.Vehicle.Chassis,

                        // Thông tin chủ xe
                        OwnerFullName = i.Owner.FullName,
                        OwnerType = i.Owner.OwnerType,
                        CompanyName = i.Owner.CompanyName,
                        OwnerPhone = i.Owner.Phone,
                        OwnerEmail = i.Owner.Email,
                        OwnerAddress = i.Owner.Address,

                        // Thông tin dây chuyền
                        LaneId = i.LaneId,
                        LaneCode = i.Lane != null ? i.Lane.LaneCode : null,
                        LaneName = i.Lane != null ? i.Lane.LaneName : null
                    })
                    .ToList();

                return records;
            }
            catch (Exception ex)
            {
                // Log error
                Console.WriteLine($"Error getting inspection records: {ex.Message}");
                return new List<InspectionRecordDto>();
            }
        }

        // Lấy chi tiết hồ sơ kiểm định
        public InspectionDetailDto? GetInspectionDetail(int inspectionId)
        {
            try
            {
                var inspection = _context.Inspections
                    .Where(i => i.InspectionId == inspectionId && !i.IsDeleted)
                    .Include(i => i.Vehicle)
                    .Include(i => i.Owner)
                    .Include(i => i.Lane)
                    .Include(i => i.Certificate)
                    .FirstOrDefault();

                if (inspection == null)
                    return null;

                // Lấy thông số kỹ thuật từ bảng Specification
                var specification = _context.Specifications
                    .FirstOrDefault(s => s.PlateNo == inspection.Vehicle.PlateNo);

                var detail = new InspectionDetailDto
                { 
                    // Thông tin hồ sơ
                    InspectionId = inspection.InspectionId,
                    InspectionCode = inspection.InspectionCode,
                    InspectionType = inspection.InspectionType,
                    Status = inspection.Status,
                    FinalResult = inspection.FinalResult,
                    CreatedAt = inspection.CreatedAt,
                    ReceivedAt = inspection.ReceivedAt,
                    PaidAt = inspection.PaidAt,
                    CompletedAt = inspection.CompletedAt,

                    // Thông tin xe
                    PlateNo = inspection.Vehicle.PlateNo,
                    InspectionNo = inspection.Vehicle.InspectionNo,
                    VehicleGroup = inspection.Vehicle.VehicleGroup,
                    VehicleType = inspection.Vehicle.VehicleType.TypeName,
                    Brand = inspection.Vehicle.Brand,
                    Model = inspection.Vehicle.Model,
                    EngineNo = inspection.Vehicle.EngineNo,
                    Chassis = inspection.Vehicle.Chassis,
                    ManufactureYear = inspection.Vehicle.ManufactureYear,
                    ManufactureCountry = inspection.Vehicle.ManufactureCountry,
                    LifetimeLimitYear = inspection.Vehicle.LifetimeLimitYear,
                    IsCleanEnergy = inspection.Vehicle.IsCleanEnergy,
                    UsagePermission = inspection.Vehicle.UsagePermission,
                    HasCommercialModification = inspection.Vehicle.HasCommercialModification,
                    HasModification = inspection.Vehicle.HasModification,

                    // Thông tin chủ xe
                    OwnerFullName = inspection.Owner.FullName,
                    OwnerType = inspection.Owner.OwnerType,
                    CompanyName = inspection.Owner.CompanyName,
                    OwnerPhone = inspection.Owner.Phone,
                    OwnerEmail = inspection.Owner.Email,
                    OwnerAddress = inspection.Owner.Address,

                    // Thông tin dây chuyền
                    LaneId = inspection.LaneId,
                    LaneCode = inspection.Lane?.LaneCode,
                    LaneName = inspection.Lane?.LaneName,

                    // Ghi chú
                    Notes = inspection.Notes
                };

                // Nếu có specification, map thêm dữ liệu
                if (specification != null)
                {
                    detail.WheelFormula = specification.WheelFormula;
                    detail.WheelTread = specification.WheelTread;
                    detail.OverallLength = specification.OverallLength;
                    detail.OverallWidth = specification.OverallWidth;
                    detail.OverallHeight = specification.OverallHeight;
                    detail.CargoInsideLength = specification.CargoInsideLength;
                    detail.CargoInsideWidth = specification.CargoInsideWidth;
                    detail.CargoInsideHeight = specification.CargoInsideHeight;
                    detail.Wheelbase = specification.Wheelbase;
                    detail.KerbWeight = specification.KerbWeight;
                    detail.AuthorizedCargoWeight = specification.AuthorizedCargoWeight;
                    detail.AuthorizedTowedWeight = specification.AuthorizedTowedWeight;
                    detail.AuthorizedTotalWeight = specification.AuthorizedTotalWeight;
                    detail.SeatingCapacity = specification.SeatingCapacity;
                    detail.StandingCapacity = specification.StandingCapacity;
                    detail.LyingCapacity = specification.LyingCapacity;
                    detail.EngineType = specification.EngineType;
                    detail.EnginePosition = specification.EnginePosition;
                    detail.EngineModel = specification.EngineModel;
                    detail.EngineDisplacement = specification.EngineDisplacement;
                    detail.MaxPower = specification.MaxPower;
                    detail.MaxPowerRPM = specification.MaxPowerRPM;
                    detail.FuelType = specification.FuelType;
                    detail.MotorType = specification.MotorType;
                    detail.NumberOfMotors = specification.NumberOfMotors;
                    detail.MotorModel = specification.MotorModel;
                    detail.TotalMotorPower = specification.TotalMotorPower;
                    detail.MotorVoltage = specification.MotorVoltage;
                    detail.BatteryType = specification.BatteryType;
                    detail.BatteryVoltage = specification.BatteryVoltage;
                    detail.BatteryCapacity = specification.BatteryCapacity;
                    detail.TireCount = specification.TireCount;
                    detail.TireSize = specification.TireSize;
                    detail.TireAxleInfo = specification.TireAxleInfo;
                    detail.HasTachograph = specification.HasTachograph;
                    detail.HasDriverCamera = specification.HasDriverCamera;
                    detail.NotIssuedStamp = specification.NotIssuedStamp;
                }

                // Nếu có certificate, map thêm dữ liệu
                if (inspection.Certificate != null)
                {
                    detail.CertificateNo = inspection.Certificate.CertificateNo;
                    detail.StickerNo = inspection.Certificate.StickerNo;
                    detail.IssueDate = inspection.Certificate.IssueDate;
                    detail.ExpiryDate = inspection.Certificate.ExpiryDate;
                }

                return detail;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting inspection detail: {ex.Message}");
                return null;
            }
        }
    }
}
