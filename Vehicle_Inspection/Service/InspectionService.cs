using Microsoft.EntityFrameworkCore;
using Vehicle_Inspection.Data;
using Vehicle_Inspection.Models;

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
                    .Where(i => !i.IsDeleted && i.Status == 2)
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

                        // Thông tin loại xe lấy từ bảng VehicleType qua VehicleId
                        //VehicleType = i.Vehicle.VehicleType != null ? i.Vehicle.VehicleType.TypeName : null,

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
                    //VehicleType = inspection.Vehicle.VehicleType.TypeName,
                    VehicleType = inspection.Vehicle.VehicleTypeId.HasValue
                        ? _context.VehicleTypes
                            .Where(vt => vt.VehicleTypeId == inspection.Vehicle.VehicleTypeId.Value)
                            .Select(vt => vt.TypeName)
                            .FirstOrDefault()
                        : null,
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

        public List<InspectionStageDto> GetInspectionStages(int inspectionId)
        {
            try
            {
                Console.WriteLine($"=== GetInspectionStages START for InspectionId: {inspectionId} ===");

                InitializeInspectionStages(inspectionId);

                // 1. Lấy thông tin Inspection và LaneId
                var inspection = _context.Inspections
                    .Where(i => i.InspectionId == inspectionId && !i.IsDeleted)
                    .Include(i => i.Vehicle)
                        .ThenInclude(v => v.VehicleType)
                    .FirstOrDefault();

                if (inspection == null)
                {
                    Console.WriteLine($"Inspection {inspectionId} not found");
                    return new List<InspectionStageDto>();
                }

                if (!inspection.LaneId.HasValue)
                {
                    Console.WriteLine($"Inspection {inspectionId} has no lane assigned");
                    return new List<InspectionStageDto>();
                }

                int laneId = inspection.LaneId.Value;
                int? vehicleTypeId = inspection.Vehicle?.VehicleTypeId;

                Console.WriteLine($"LaneId: {laneId}, VehicleTypeId: {vehicleTypeId}");

                // 2. Lấy các Stage theo LaneId từ LaneStage
                var laneStages = _context.LaneStages
                    .Where(ls => ls.LaneId == laneId && ls.IsActive == true)
                    .Include(ls => ls.Stage)
                    .OrderBy(ls => ls.SortOrder)
                    .Select(ls => new
                    {
                        ls.StageId,
                        ls.Stage.StageCode,
                        ls.Stage.StageName,
                        ls.SortOrder,
                        ls.IsRequired
                    })
                    .ToList();

                Console.WriteLine($"Found {laneStages.Count} stages for lane {laneId}");

                // 3. Lấy thông tin InspectionStage đã có (nếu có)
                var existingStages = _context.InspectionStages
                    .Where(ins => ins.InspectionId == inspectionId)
                    .Include(ins => ins.AssignedUser)
                    .ToDictionary(ins => ins.StageId);

                // 4. Build DTO cho từng Stage
                var result = new List<InspectionStageDto>();

                foreach (var ls in laneStages)
                {
                    var stageDto = new InspectionStageDto
                    {
                        StageId = ls.StageId,
                        StageCode = ls.StageCode,
                        StageName = ls.StageName,
                        SortOrder = ls.SortOrder,
                        IsRequired = ls.IsRequired ?? true
                    };

                    // Nếu đã có InspectionStage, map thông tin
                    if (existingStages.TryGetValue(ls.StageId, out var existingStage))
                    {
                        stageDto.InspStageId = existingStage.InspStageId;
                        stageDto.Status = existingStage.Status;
                        stageDto.StageResult = existingStage.StageResult;
                        stageDto.AssignedUserId = existingStage.AssignedUserId;
                        stageDto.AssignedUserName = existingStage.AssignedUser?.FullName;
                        stageDto.Notes = existingStage.Notes;
                    }
                    else
                    {
                        stageDto.Status = 0; // Pending
                    }

                    // 5. Lấy danh sách StageItem
                    var stageItems = _context.StageItems
                        .Where(si => si.StageId == ls.StageId)
                        .OrderBy(si => si.SortOrder)
                        .ToList();

                    Console.WriteLine($"Stage {ls.StageName} has {stageItems.Count} items");

                    foreach (var item in stageItems)
                    {
                        var itemDto = new StageItemDto
                        {
                            ItemId = item.ItemId,
                            ItemCode = item.ItemCode,
                            ItemName = item.ItemName,
                            Unit = item.Unit,
                            DataType = item.DataType,
                            IsRequired = item.IsRequired,
                            SortOrder = item.SortOrder ?? 0
                        };

                        // 6. Lấy tiêu chuẩn từ StageItemThreshold (nếu có VehicleTypeId)
                        if (vehicleTypeId.HasValue)
                        {
                            var threshold = _context.StageItemThresholds
                                .Where(t => t.ItemId == item.ItemId
                                         && t.VehicleTypeId == vehicleTypeId.Value
                                         && t.IsActive == true)
                                .OrderByDescending(t => t.EffectiveDate)
                                .FirstOrDefault();

                            if (threshold != null)
                            {
                                itemDto.MinValue = threshold.MinValue;
                                itemDto.MaxValue = threshold.MaxValue;
                                itemDto.AllowedValues = threshold.AllowedValues;
                                itemDto.PassCondition = threshold.PassCondition;
                            }
                        }

                        // 7. Lấy giá trị đã đo (nếu có InspectionDetail)
                        if (stageDto.InspStageId.HasValue)
                        {
                            var detail = _context.InspectionDetails
                                .FirstOrDefault(d => d.InspStageId == stageDto.InspStageId.Value
                                                  && d.ItemId == item.ItemId);

                            if (detail != null)
                            {
                                itemDto.ActualValue = detail.ActualValue;
                                //itemDto.ActualText = detail.ActualText;
                                itemDto.IsPassed = detail.IsPassed;
                            }
                        }

                        stageDto.Items.Add(itemDto);
                    }

                    result.Add(stageDto);
                }

                Console.WriteLine($"=== GetInspectionStages END - Returning {result.Count} stages ===");
                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in GetInspectionStages: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return new List<InspectionStageDto>();
            }
        }

        public bool InitializeInspectionStages(int inspectionId)
        {
            try
            {
                Console.WriteLine($"=== InitializeInspectionStages for InspectionId: {inspectionId} ===");

                // 1. Lấy thông tin Inspection
                var inspection = _context.Inspections
                    .FirstOrDefault(i => i.InspectionId == inspectionId && !i.IsDeleted);

                if (inspection == null)
                {
                    Console.WriteLine($"Inspection {inspectionId} not found");
                    return false;
                }

                if (!inspection.LaneId.HasValue)
                {
                    Console.WriteLine($"Inspection {inspectionId} has no lane assigned");
                    return false;
                }

                int laneId = inspection.LaneId.Value;
                Console.WriteLine($"LaneId: {laneId}");

                // 2. Kiểm tra xem đã có InspectionStage chưa
                var existingCount = _context.InspectionStages
                    .Count(ins => ins.InspectionId == inspectionId);

                if (existingCount > 0)
                {
                    Console.WriteLine($"InspectionStages already exist ({existingCount} records). Skipping initialization.");
                    return true; // Đã có rồi, bỏ qua
                }

                // 3. Lấy cấu hình LaneStage
                var laneStages = _context.LaneStages
                    .Where(ls => ls.LaneId == laneId && ls.IsActive == true)
                    .OrderBy(ls => ls.SortOrder)
                    .ToList();

                if (laneStages.Count == 0)
                {
                    Console.WriteLine($"No active LaneStages found for LaneId {laneId}");
                    return false;
                }

                Console.WriteLine($"Found {laneStages.Count} LaneStages to initialize");

                // 4. Tạo các InspectionStage mới
                var newInspectionStages = new List<InspectionStage>();

                foreach (var ls in laneStages)
                {
                    var inspStage = new InspectionStage
                    {
                        InspectionId = inspectionId,
                        StageId = ls.StageId,
                        Status = 0,  // PENDING
                        StageResult = null,
                        AssignedUserId = null,
                        SortOrder = ls.SortOrder,
                        IsRequired = ls.IsRequired ?? true,
                        Notes = null
                    };

                    newInspectionStages.Add(inspStage);
                    Console.WriteLine($"Creating InspectionStage: StageId={ls.StageId}, SortOrder={ls.SortOrder}");
                }

                // 5. Lưu vào database
                _context.InspectionStages.AddRange(newInspectionStages);
                _context.SaveChanges();

                Console.WriteLine($"✅ Successfully created {newInspectionStages.Count} InspectionStages");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in InitializeInspectionStages: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                return false;
            }
        }
    }
}
