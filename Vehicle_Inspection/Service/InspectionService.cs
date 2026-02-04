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
                    .Include(i => i.Vehicle)
                    .ThenInclude(v => v.VehicleType)
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
                        VehicleTypeId = i.Vehicle.VehicleTypeId,
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

                Console.WriteLine($"📋 VehicleTypeId: {vehicleTypeId?.ToString() ?? "NULL"}");

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

                var existingStages = _context.InspectionStages
                    .Where(ins => ins.InspectionId == inspectionId)
                    .ToDictionary(ins => ins.StageId);

                var stageUserMapping = _context.Users
                    .Where(u => u.IsActive)
                    .SelectMany(u => u.Stages.Select(s => new
                    {
                        StageId = s.StageId,
                        UserName = u.FullName
                    }))
                    .GroupBy(x => x.StageId)
                    .ToDictionary(
                        g => g.Key,
                        g => string.Join(", ", g.Select(x => x.UserName))
                    );

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

                    if (stageUserMapping.TryGetValue(ls.StageId, out var assignedUsers))
                    {
                        stageDto.AssignedUserName = assignedUsers;
                    }

                    var stageItems = _context.StageItems
                        .Where(si => si.StageId == ls.StageId)
                        .OrderBy(si => si.SortOrder)
                        .ToList();

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

                        StageItemThreshold? threshold = null;

                        // ✅ TÌM THRESHOLD CHO VEHICLE TYPE CỤ THỂ
                        if (vehicleTypeId.HasValue)
                        {
                            threshold = _context.StageItemThresholds
                                .Where(t => t.ItemId == item.ItemId
                                         && t.VehicleTypeId == vehicleTypeId.Value
                                         && t.IsActive == true)
                                .OrderByDescending(t => t.EffectiveDate)
                                .FirstOrDefault();
                        }

                        // ✅ FALLBACK: Nếu không tìm thấy, lấy tiêu chuẩn chung
                        if (threshold == null)
                        {
                            threshold = _context.StageItemThresholds
                                .Where(t => t.ItemId == item.ItemId
                                         && t.VehicleTypeId == null
                                         && t.IsActive == true)
                                .OrderByDescending(t => t.EffectiveDate)
                                .FirstOrDefault();
                        }

                        // ✅ SET HasThreshold FLAG
                        itemDto.HasThreshold = threshold != null;

                        if (threshold != null)
                        {
                            itemDto.MinValue = threshold.MinValue;
                            itemDto.MaxValue = threshold.MaxValue;
                            itemDto.PassCondition = threshold.PassCondition;
                            itemDto.AllowedValues = threshold.AllowedValues;
                        }

                        // ✅ LẤY GIÁ TRỊ ĐÃ ĐO
                        if (stageDto.InspStageId.HasValue)
                        {
                            var detail = _context.InspectionDetails
                                .FirstOrDefault(d => d.InspStageId == stageDto.InspStageId.Value
                                                  && d.ItemId == item.ItemId);

                            if (detail != null)
                            {
                                itemDto.ActualValue = detail.ActualValue;
                                itemDto.ActualText = detail.ActualText;
                                itemDto.IsPassed = detail.IsPassed;
                            }
                        }

                        stageDto.Items.Add(itemDto);
                    }

                    result.Add(stageDto);
                }

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error in GetInspectionStages: {ex.Message}");
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
                Console.WriteLine($"UserId: {request.UserId}");

                // 1. Kiểm tra InspectionStage tồn tại
                var inspStage = _context.InspectionStages
                    .Include(ins => ins.Stage)
                    .FirstOrDefault(ins => ins.InspStageId == request.InspStageId
                                        && ins.InspectionId == request.InspectionId);

                if (inspStage == null)
                {
                    Console.WriteLine("❌ InspectionStage not found");
                    return false;
                }

                // ✅ 2. KIỂM TRA QUYỀN
                var hasPermission = CheckUserStagePermission(request.UserId, inspStage.StageId);

                if (!hasPermission)
                {
                    Console.WriteLine($"❌ User {request.UserId} is NOT authorized for StageId {inspStage.StageId}");
                    return false;
                }

                Console.WriteLine($"✅ User {request.UserId} is authorized");

                // 3. Lưu measurements
                int passedCount = 0;
                int failedCount = 0;
                var defectsToAdd = new List<InspectionDefect>();

                foreach (var measurement in request.Measurements)
                {
                    var existingDetail = _context.InspectionDetails
                        .FirstOrDefault(d => d.InspStageId == request.InspStageId
                                          && d.ItemId == measurement.ItemId);

                    if (existingDetail != null)
                    {
                        _context.InspectionDetails.Remove(existingDetail);
                    }

                    var detail = new InspectionDetail
                    {
                        InspStageId = request.InspStageId,
                        ItemId = measurement.ItemId,
                        StandardMin = measurement.StandardMin,
                        StandardMax = measurement.StandardMax,
                        ActualValue = measurement.ActualValue,
                        ActualText = measurement.ActualText,
                        Unit = measurement.Unit,
                        IsPassed = measurement.IsPassed,
                        DataSource = "MANUAL",
                        RecordedAt = DateTime.Now
                    };

                    _context.InspectionDetails.Add(detail);

                    if (measurement.IsPassed)
                    {
                        passedCount++;
                    }
                    else
                    {
                        failedCount++;

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
                                Severity = measurement.DefectSeverity ?? 2,
                                ImageUrls = null,
                                IsFixed = false
                            };

                            defectsToAdd.Add(defect);
                        }
                    }
                }

                _context.SaveChanges();

                if (defectsToAdd.Count > 0)
                {
                    _context.InspectionDefects.AddRange(defectsToAdd);
                    _context.SaveChanges();
                }

                inspStage.Status = 2;
                inspStage.StageResult = failedCount > 0 ? 2 : 1;
                inspStage.Notes = request.Notes;

                _context.SaveChanges();

                var allStagesCompleted = _context.InspectionStages
                    .Where(ins => ins.InspectionId == request.InspectionId)
                    .All(ins => ins.Status == 2);

                if (allStagesCompleted)
                {
                    var inspection = _context.Inspections
                        .FirstOrDefault(i => i.InspectionId == request.InspectionId);

                    if (inspection != null)
                    {
                        inspection.Status = 4;
                        inspection.CompletedAt = DateTime.Now;
                        _context.SaveChanges();
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


        //Lấy dây chuyền phù hợp với loại xe
        public List<Lane> GetSuitableLanes(int vehicleTypeId)
        {
            try
            {
                var lanes = _context.Lanes
                    .Where(l => l.IsActive && l.VehicleTypes.Any(vt => vt.VehicleTypeId == vehicleTypeId))
                    .OrderBy(l => l.LaneCode)
                    .ToList();

                Console.WriteLine($"Found {lanes.Count} suitable lanes for VehicleTypeId {vehicleTypeId}");
                return lanes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting suitable lanes: {ex.Message}");
                return new List<Lane>();
            }
        }

        public bool CheckUserStagePermission(Guid userId, int stageId)
        {
            try
            {
                Console.WriteLine($"=== CheckUserStagePermission ===");
                Console.WriteLine($"UserId: {userId}");
                Console.WriteLine($"StageId: {stageId}");

                // Kiểm tra trong bảng trung gian User-Stage (Many-to-Many)
                var hasPermission = _context.Users
                    .Where(u => u.UserId == userId && u.IsActive)
                    .SelectMany(u => u.Stages)
                    .Any(s => s.StageId == stageId && s.IsActive == true);

                Console.WriteLine($"HasPermission: {hasPermission}");
                return hasPermission;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CheckUserStagePermission: {ex.Message}");
                return false;
            }
        }
    }
}
