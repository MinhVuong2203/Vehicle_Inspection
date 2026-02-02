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
                    .Where(i => !i.IsDeleted && (i.Status == 2 || i.Status == 3))
                    .Include(i => i.Vehicle)
                     .ThenInclude(v => v.Owner)
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
                        OwnerFullName = i.Vehicle.Owner.FullName,
                        OwnerType = i.Vehicle.Owner.OwnerType,
                        CompanyName = i.Vehicle.Owner.CompanyName,
                        OwnerPhone = i.Vehicle.Owner.Phone,
                        OwnerEmail = i.Vehicle.Owner.Email,
                        OwnerAddress = i.Vehicle.Owner.Address,

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
                    //.Include(i => i.Owner)
                    .ThenInclude(v => v.Owner)
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
                    OwnerFullName = inspection.Vehicle.Owner.FullName,
                    OwnerType = inspection.Vehicle.Owner.OwnerType,
                    CompanyName = inspection.Vehicle.Owner.CompanyName,
                    OwnerPhone = inspection.Vehicle.Owner.Phone,
                    OwnerEmail = inspection.Vehicle.Owner.Email,
                    OwnerAddress = inspection.Vehicle.Owner.Address,

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
                    Console.WriteLine($"❌ Inspection {inspectionId} not found");
                    return new List<InspectionStageDto>();
                }

                if (!inspection.LaneId.HasValue)
                {
                    Console.WriteLine($"❌ Inspection {inspectionId} has no lane assigned");
                    return new List<InspectionStageDto>();
                }

                int laneId = inspection.LaneId.Value;
                int? vehicleTypeId = inspection.Vehicle?.VehicleTypeId;

                // ✅ LOG THÔNG TIN CHỦ CHỐT
                Console.WriteLine($"📋 InspectionId: {inspectionId}");
                Console.WriteLine($"📋 InspectionCode: {inspection.InspectionCode}");
                Console.WriteLine($"📋 PlateNo: {inspection.Vehicle?.PlateNo}");
                Console.WriteLine($"📋 LaneId: {laneId}");
                Console.WriteLine($"📋 VehicleTypeId: {vehicleTypeId?.ToString() ?? "NULL"}");
                Console.WriteLine($"📋 VehicleTypeName: {inspection.Vehicle?.VehicleType?.TypeName ?? "NULL"}");

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

                var user = _context.Users
                    .Include(u => u.Stages);

                // 4. Build DTO cho từng Stage
                var result = new List<InspectionStageDto>();

                var stageUserMapping = _context.Users
                    .Where(u => u.IsActive) // Chỉ lấy user còn hoạt động
                    .SelectMany(u => u.Stages.Select(s => new
                    {
                        StageId = s.StageId,
                        UserName = u.FullName
                    }))
                    .GroupBy(x => x.StageId)
                    .ToDictionary(
                        g => g.Key,
                        g => string.Join(", ", g.Select(x => x.UserName)) // ✅ Gộp nhiều user thành chuỗi
                    );

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

                    // Nếu đã có InspectionStage, map thông tin kèm tên và mã nhân viên được giao
                    if (existingStages.TryGetValue(ls.StageId, out var existingStage))
                    {
                        stageDto.InspStageId = existingStage.InspStageId;
                        stageDto.Status = existingStage.Status;
                        stageDto.StageResult = existingStage.StageResult;
                        stageDto.Notes = existingStage.Notes;
                    }
                    else
                    {
                        stageDto.Status = 0;
                    }

                    // ✅ LẤY TÊN NHÂN VIÊN TỪ UserStage (áp dụng chung cho tất cả hồ sơ)
                    if (stageUserMapping.TryGetValue(ls.StageId, out var assignedUsers))
                    {
                        stageDto.AssignedUserName = assignedUsers; // ✅ Có thể là "User1, User2, User3"
                        Console.WriteLine($"✅ Stage {ls.StageName}: AssignedUsers = {assignedUsers}");
                    }
                    else
                    {
                        stageDto.AssignedUserName = null;
                        Console.WriteLine($"⚠️ Stage {ls.StageName}: No users assigned in UserStage");
                    }

                    // 5. Lấy danh sách StageItem
                    var stageItems = _context.StageItems
                        .Where(si => si.StageId == ls.StageId)
                        .OrderBy(si => si.SortOrder)
                        .ToList();

                    Console.WriteLine($"🔹 Stage {ls.StageName} (StageId={ls.StageId}) has {stageItems.Count} items");

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

                        // 7. Lấy tiêu chuẩn từ StageItemThreshold
                        StageItemThreshold? threshold = null;

                        if (vehicleTypeId.HasValue)
                        {
                            threshold = _context.StageItemThresholds
                                .Where(t => t.ItemId == item.ItemId
                                         && t.VehicleTypeId == vehicleTypeId.Value
                                         && t.IsActive == true)
                                .OrderByDescending(t => t.EffectiveDate)
                                .FirstOrDefault();
                        }

                        // Fallback: Nếu không tìm thấy, lấy tiêu chuẩn chung
                        if (threshold == null)
                        {
                            threshold = _context.StageItemThresholds
                                .Where(t => t.ItemId == item.ItemId
                                         && t.VehicleTypeId == null
                                         && t.IsActive == true)
                                .OrderByDescending(t => t.EffectiveDate)
                                .FirstOrDefault();
                        }

                        if (threshold != null)
                        {
                            itemDto.MinValue = threshold.MinValue;
                            itemDto.MaxValue = threshold.MaxValue;
                            itemDto.PassCondition = threshold.PassCondition;
                            itemDto.AllowedValues = threshold.AllowedValues;
                        }

                        // 8. Lấy giá trị đã đo (nếu có InspectionDetail)
                        if (stageDto.InspStageId.HasValue)
                        {
                            var detail = _context.InspectionDetails
                                .FirstOrDefault(d => d.InspStageId == stageDto.InspStageId.Value
                                                  && d.ItemId == item.ItemId);

                            if (detail != null)
                            {
                                itemDto.ActualValue = detail.ActualValue;
                                itemDto.ActualText = detail.ActualText;  // ✅ LẤY TEXT ĐÃ LƯU
                                itemDto.IsPassed = detail.IsPassed;

                                Console.WriteLine($"✅ Item {item.ItemName}: ActualValue={detail.ActualValue}, ActualText={detail.ActualText}, IsPassed={detail.IsPassed}");
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
                Console.WriteLine($"❌ Error in GetInspectionStages: {ex.Message}");
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

        // Lưu kết quả của một stage kiểm định
        public bool SaveStageResult(SaveStageResultRequest request)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                Console.WriteLine($"=== SaveStageResult START ===");
                Console.WriteLine($"InspectionId: {request.InspectionId}");
                Console.WriteLine($"InspStageId: {request.InspStageId}");
                Console.WriteLine($"Measurements count: {request.Measurements.Count}");

                // 1. Kiểm tra InspectionStage tồn tại
                var inspStage = _context.InspectionStages
                    .Include(ins => ins.Stage)
                    .FirstOrDefault(ins => ins.InspStageId == request.InspStageId
                                        && ins.InspectionId == request.InspectionId);

                if (inspStage == null)
                {
                    Console.WriteLine("InspectionStage not found");
                    return false;
                }

                // 2. Lưu từng measurement vào InspectionDetail
                int passedCount = 0;
                int failedCount = 0;
                var defectsToAdd = new List<InspectionDefect>();

                foreach (var measurement in request.Measurements)
                {
                    // 2.1. Xóa InspectionDetail cũ nếu có (update)
                    var existingDetail = _context.InspectionDetails
                        .FirstOrDefault(d => d.InspStageId == request.InspStageId
                                          && d.ItemId == measurement.ItemId);

                    if (existingDetail != null)
                    {
                        _context.InspectionDetails.Remove(existingDetail);
                    }

                    // 2.2. Tạo InspectionDetail mới - CHỈ CÁC CỘT CÒN TỒN TẠI
                    var detail = new InspectionDetail
                    {
                        InspStageId = request.InspStageId,
                        ItemId = measurement.ItemId,

                        // ✅ Chỉ giữ các cột còn tồn tại
                        StandardMin = measurement.StandardMin,
                        StandardMax = measurement.StandardMax,
                        ActualValue = measurement.ActualValue,
                        ActualText = measurement.ActualText,

                        Unit = measurement.Unit,
                        IsPassed = measurement.IsPassed,
                        DataSource = "MANUAL",
                        RecordedAt = DateTime.Now

                        // ❌ XÓA các cột sau (đã bị xóa khỏi database):
                        // ActualText - đã xóa
                        // StandardText - đã xóa
                        // DeviationPercent - đã xóa
                        // DeviceId - đã xóa
                        // RecordedBy - đã xóa
                        // ImageUrls - đã xóa
                        // Notes - đã xóa
                    };

                    _context.InspectionDetails.Add(detail);

                    Console.WriteLine($"  - Item {measurement.ItemName}: " +
                                    $"Actual={measurement.ActualValue}, " +
                                    $"IsPassed={measurement.IsPassed}");

                    // 2.3. Đếm số lượng đạt/không đạt
                    if (measurement.IsPassed)
                    {
                        passedCount++;
                    }
                    else
                    {
                        failedCount++;

                        // 2.4. Tạo InspectionDefect nếu không đạt
                        if (!string.IsNullOrEmpty(measurement.DefectDescription))
                        {
                            var defect = new InspectionDefect
                            {
                                InspectionId = request.InspectionId,
                                InspStageId = request.InspStageId,
                                ItemId = measurement.ItemId,
                                DefectCategory = measurement.DefectCategory ?? inspStage.Stage?.StageName ?? "Lỗi chung",
                                DefectCode = measurement.ItemCode,
                                DefectDescription = measurement.DefectDescription,
                                Severity = measurement.DefectSeverity ?? 2, // Default: Major
                                ImageUrls = null,
                                IsFixed = false

                                // ❌ XÓA: CreatedBy - đã bị xóa
                            };

                            defectsToAdd.Add(defect);

                            Console.WriteLine($"  - Created defect: {defect.DefectDescription}");
                        }
                    }
                }

                // 3. Lưu InspectionDetails
                _context.SaveChanges();
                Console.WriteLine($"✅ Saved {request.Measurements.Count} InspectionDetails");

                // 4. Lưu InspectionDefects (nếu có)
                if (defectsToAdd.Count > 0)
                {
                    _context.InspectionDefects.AddRange(defectsToAdd);
                    _context.SaveChanges();
                    Console.WriteLine($"✅ Saved {defectsToAdd.Count} InspectionDefects");
                }

                // 5. Cập nhật InspectionStage
                inspStage.Status = 2; // COMPLETED
                inspStage.StageResult = failedCount > 0 ? 2 : 1; // FAILED : PASSED
                inspStage.Notes = request.Notes;

                _context.SaveChanges();
                Console.WriteLine($"✅ Updated InspectionStage: Status=2, Result={inspStage.StageResult}");

                // 6. Kiểm tra xem tất cả stages đã hoàn thành chưa
                var allStagesCompleted = _context.InspectionStages
                    .Where(ins => ins.InspectionId == request.InspectionId)
                    .All(ins => ins.Status == 2);

                if (allStagesCompleted)
                {
                    // Cập nhật Inspection status
                    var inspection = _context.Inspections
                        .FirstOrDefault(i => i.InspectionId == request.InspectionId);

                    if (inspection != null)
                    {
                        inspection.Status = 4; // COMPLETED
                        inspection.CompletedAt = DateTime.Now;
                        _context.SaveChanges();
                        Console.WriteLine("✅ All stages completed. Updated Inspection status to COMPLETED");
                    }
                }

                transaction.Commit();
                Console.WriteLine($"=== SaveStageResult END - SUCCESS ===");
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"❌ Error in SaveStageResult: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                // ✅ LOG CHI TIẾT LỖI DATABASE
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"❌ Inner Exception: {ex.InnerException.Message}");
                }

                return false;
            }
        }

        // Lấy danh sách lỗi của một stage trong kiểm định
        public List<InspectionDefectDto> GetStageDefects(int inspectionId, int stageId)
        {
            try
            {
                var defects = _context.InspectionDefects
                    .Where(d => d.InspectionId == inspectionId)
                    .Include(d => d.InspStage)
                    .Where(d => d.InspStage.StageId == stageId)
                    .Select(d => new InspectionDefectDto
                    {
                        DefectId = d.DefectId,
                        StageId = stageId,
                        DefectCategory = d.DefectCategory,
                        DefectCode = d.DefectCode,
                        DefectDescription = d.DefectDescription,
                        Severity = d.Severity,
                        IsFixed = d.IsFixed
                    })
                    .ToList();

                return defects;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting defects: {ex.Message}");
                return new List<InspectionDefectDto>();
            }
        }

        // Nộp kết quả kiểm định
        public bool SubmitInspectionResult(SubmitInspectionResultRequest request)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                Console.WriteLine($"=== SubmitInspectionResult START ===");
                Console.WriteLine($"InspectionId: {request.InspectionId}");
                Console.WriteLine($"FinalResult: {request.FinalResult}");

                // 1. Tìm Inspection
                var inspection = _context.Inspections
                    .FirstOrDefault(i => i.InspectionId == request.InspectionId && !i.IsDeleted);

                if (inspection == null)
                {
                    Console.WriteLine("Inspection not found");
                    return false;
                }

                // 2. Kiểm tra tất cả stages đã hoàn thành chưa
                var allStagesCompleted = _context.InspectionStages
                    .Where(ins => ins.InspectionId == request.InspectionId)
                    .All(ins => ins.Status == 2); // 2 = COMPLETED

                if (!allStagesCompleted)
                {
                    Console.WriteLine("Not all stages are completed");
                    return false;
                }

                // 3. Cập nhật Inspection
                inspection.Status = 4; // COMPLETED
                inspection.FinalResult = request.FinalResult;
                inspection.ConclusionNote = request.ConclusionNote;
                inspection.CompletedAt = DateTime.Now;

                // TODO: Nếu có thông tin người kết luận, cập nhật ConcludedBy và ConcludedAt
                // inspection.ConcludedBy = userId;
                // inspection.ConcludedAt = DateTime.Now;

                _context.SaveChanges();
                Console.WriteLine($"✅ Updated Inspection: Status=4, FinalResult={request.FinalResult}");

                transaction.Commit();
                Console.WriteLine($"=== SubmitInspectionResult END - SUCCESS ===");
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"❌ Error in SubmitInspectionResult: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"❌ Inner Exception: {ex.InnerException.Message}");
                }

                return false;
            }
        }

        public List<Lane> GetInspectionLanes()
        {
            try
            {
                var lanes = _context.Lanes
                    .Where(l => l.IsActive)
                    .OrderBy(l => l.LaneCode)
                    .ToList();
                return lanes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting lanes: {ex.Message}");
                return new List<Lane>();
            }
        }

        public bool AssignLane(AssignLaneRequest request)
        {
            using var transaction = _context.Database.BeginTransaction();

            try
            {
                Console.WriteLine($"=== AssignLane START ===");
                Console.WriteLine($"InspectionId: {request.InspectionId}");
                Console.WriteLine($"LaneId: {request.LaneId}");

                // 1. Kiểm tra Inspection tồn tại và có status phù hợp
                var inspection = _context.Inspections
                    .FirstOrDefault(i => i.InspectionId == request.InspectionId && !i.IsDeleted);

                if (inspection == null)
                {
                    Console.WriteLine("Inspection not found");
                    return false;
                }

                // Kiểm tra trạng thái (chỉ cho phép gán khi Status = 2: PAID)
                if (inspection.Status != 2)
                {
                    Console.WriteLine($"Invalid status: {inspection.Status}. Only status 2 (PAID) can be assigned.");
                    return false;
                }

                // 2. Kiểm tra Lane tồn tại và active
                var lane = _context.Lanes
                    .FirstOrDefault(l => l.LaneId == request.LaneId && l.IsActive);

                if (lane == null)
                {
                    Console.WriteLine("Lane not found or inactive");
                    return false;
                }

                // 3. Cập nhật LaneId cho Inspection
                inspection.LaneId = request.LaneId;
                inspection.Status = 3; // IN_PROGRESS - Đang kiểm định
                inspection.StartedAt = DateTime.Now;

                // Nếu có ghi chú, thêm vào Notes
                if (!string.IsNullOrEmpty(request.Note))
                {
                    inspection.Notes = string.IsNullOrEmpty(inspection.Notes)
                        ? $"Phân công: {request.Note}"
                        : $"{inspection.Notes}\nPhân công: {request.Note}";
                }

                _context.SaveChanges();
                Console.WriteLine($"Updated Inspection: LaneId={request.LaneId}, Status=3");

                // 4. Khởi tạo InspectionStages nếu chưa có
                var existingStagesCount = _context.InspectionStages
                    .Count(ins => ins.InspectionId == request.InspectionId);

                if (existingStagesCount == 0)
                {
                    Console.WriteLine("Initializing InspectionStages...");
                    InitializeInspectionStages(request.InspectionId);
                }
                else
                {
                    Console.WriteLine($"InspectionStages already exist ({existingStagesCount} records)");
                }

                transaction.Commit();
                Console.WriteLine($"=== AssignLane END - SUCCESS ===");
                return true;
            }
            catch (Exception ex)
            {
                transaction.Rollback();
                Console.WriteLine($"Error in AssignLane: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }

                return false;
            }
        }
    }
}
