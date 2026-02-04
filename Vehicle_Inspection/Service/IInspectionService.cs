using Vehicle_Inspection.Models;

namespace Vehicle_Inspection.Service
{
    public interface IInspectionService
    {
        List<InspectionRecordDto> GetInspectionRecords();
        InspectionDetailDto? GetInspectionDetail(int inspectionId);

        //lấy các bước kiểm định theo dây chuyền được gán với hồ sơ kiểm định
        List<InspectionStageDto> GetInspectionStages(int inspectionId);

        //Tạo các bước kiểm định cho hồ sơ
        bool InitializeInspectionStages(int inspectionId);

        //Lưu kết quả của bước kiểm định
        bool SaveStageResult(SaveStageResultRequest request);

        // Lấy danh sách các lỗi phát hiện trong bước kiểm định
        List<InspectionDefectDto> GetStageDefects(int inspectionId, int stageId);

        // Nộp kết quả kiểm định
        bool SubmitInspectionResult(SubmitInspectionResultRequest request);

        //lấy danh sách các luồng kiểm định
        List<Lane> GetInspectionLanes();

        //Lấy dây chuyền phù hợp với loại xe
        List<Lane> GetSuitableLanes(int vehicleTypeId);

        bool CheckUserStagePermission(Guid userId, int stageId);

        bool AssignLane(AssignLaneRequest request);


        //List<User> GetUsersStage(int stageId);
    }

    // DTO cho danh sách
    public class InspectionRecordDto
    {
        public int InspectionId { get; set; }
        public string? InspectionCode { get; set; }
        public string? InspectionType { get; set; }
        public short Status { get; set; }
        public int? FinalResult { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ReceivedAt { get; set; }
        public DateTime? PaidAt { get; set; }
        public DateTime? CompletedAt { get; set; }

        // Thông tin xe
        public string? PlateNo { get; set; }
        public string? InspectionNo { get; set; }
        public string? VehicleGroup { get; set; }
        public string? VehicleType { get; set; }
        public int? VehicleTypeId { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }
        public string? EngineNo { get; set; }
        public string? Chassis { get; set; }

        // Thông tin chủ xe
        public string? OwnerFullName { get; set; }
        public string? OwnerType { get; set; }
        public string? CompanyName { get; set; }
        public string? OwnerPhone { get; set; }
        public string? OwnerEmail { get; set; }
        public string? OwnerAddress { get; set; }

        // Thông tin dây chuyền
        public int? LaneId { get; set; }
        public string? LaneCode { get; set; }
        public string? LaneName { get; set; }

        // Hiển thị
        public string StatusText => GetStatusText();
        public string InspectionTypeText => GetInspectionTypeText();
        public string FinalResultText => GetFinalResultText();

        private string GetStatusText()
        {
            return Status switch
            {
                0 => "Nháp",
                1 => "Đã tiếp nhận",
                2 => "Đã thu phí",
                3 => "Đang kiểm định",
                4 => "Hoàn thành",
                5 => "Đạt",
                6 => "Không đạt",
                7 => "Đã cấp chứng nhận",
                8 => "Đã hủy",
                _ => "Không xác định"
            };
        }

        private string GetInspectionTypeText()
        {
            return InspectionType switch
            {
                "FIRST" => "Lần đầu",
                "PERIODIC" => "Định kỳ",
                "RE_INSPECTION" => "Tái kiểm",
                _ => "Không xác định"
            };
        }

        private string GetFinalResultText()
        {
            if (FinalResult == null) return "Chưa có kết luận";
            return FinalResult switch
            {
                1 => "Đạt",
                2 => "Không đạt",
                3 => "Tạm đình chỉ",
                _ => "Không xác định"
            };
        }
    }

    // DTO chi tiết cho modal (kèm Specification)
    public class InspectionDetailDto : InspectionRecordDto
    {
        // Thông tin từ Vehicle
        public int? ManufactureYear { get; set; }
        public string? ManufactureCountry { get; set; }
        public int? LifetimeLimitYear { get; set; }
        public bool? IsCleanEnergy { get; set; }
        public string? UsagePermission { get; set; }
        public bool? HasCommercialModification { get; set; }
        public bool? HasModification { get; set; }

        // Thông tin từ Specification
        public string? WheelFormula { get; set; }
        public int? WheelTread { get; set; }
        public int? OverallLength { get; set; }
        public int? OverallWidth { get; set; }
        public int? OverallHeight { get; set; }
        public int? CargoInsideLength { get; set; }
        public int? CargoInsideWidth { get; set; }
        public int? CargoInsideHeight { get; set; }
        public int? Wheelbase { get; set; }
        public decimal? KerbWeight { get; set; }
        public decimal? AuthorizedCargoWeight { get; set; }
        public decimal? AuthorizedTowedWeight { get; set; }
        public decimal? AuthorizedTotalWeight { get; set; }
        public int? SeatingCapacity { get; set; }
        public int? StandingCapacity { get; set; }
        public int? LyingCapacity { get; set; }
        public string? EngineType { get; set; }
        public string? EnginePosition { get; set; }
        public string? EngineModel { get; set; }
        public int? EngineDisplacement { get; set; }
        public decimal? MaxPower { get; set; }
        public int? MaxPowerRPM { get; set; }
        public string? FuelType { get; set; }
        public string? MotorType { get; set; }
        public int? NumberOfMotors { get; set; }
        public string? MotorModel { get; set; }
        public decimal? TotalMotorPower { get; set; }
        public decimal? MotorVoltage { get; set; }
        public string? BatteryType { get; set; }
        public decimal? BatteryVoltage { get; set; }
        public decimal? BatteryCapacity { get; set; }
        public int? TireCount { get; set; }
        public string? TireSize { get; set; }
        public string? TireAxleInfo { get; set; }
        public bool? HasTachograph { get; set; }
        public bool? HasDriverCamera { get; set; }
        public bool? NotIssuedStamp { get; set; }
        public string? Notes { get; set; }

        // Thông tin từ Certificate (nếu có)
        public string? CertificateNo { get; set; }
        public string? StickerNo { get; set; }
        public DateOnly? IssueDate { get; set; }
        public DateOnly? ExpiryDate { get; set; }

        // Computed properties
        public string? OverallDimensions => 
            OverallLength.HasValue && OverallWidth.HasValue && OverallHeight.HasValue
            ? $"{OverallLength} x {OverallWidth} x {OverallHeight}"
            : null;

        public string? CargoInsideDimensions =>
            CargoInsideLength.HasValue && CargoInsideWidth.HasValue && CargoInsideHeight.HasValue
            ? $"{CargoInsideLength} x {CargoInsideWidth} x {CargoInsideHeight}"
            : null;

        public string? MaxOutputRPM =>
            MaxPower.HasValue && MaxPowerRPM.HasValue
            ? $"{MaxPower}/{MaxPowerRPM}"
            : null;

        public string? MotorPowerInfo =>
            MotorVoltage.HasValue && TotalMotorPower.HasValue
            ? $"{MotorVoltage}/{TotalMotorPower}"
            : null;

        public string? BatteryInfo =>
            BatteryVoltage.HasValue && BatteryCapacity.HasValue
            ? $"{BatteryVoltage}-{BatteryCapacity}"
            : null;

        public string? ProductionInfo =>
            ManufactureYear.HasValue && !string.IsNullOrEmpty(ManufactureCountry)
            ? $"{ManufactureYear}/{ManufactureCountry}"
            : null;
    }

    public class InspectionStageDto
    {
        public int StageId { get; set; }
        public string StageCode { get; set; }
        public string StageName { get; set; }
        public int SortOrder { get; set; }
        public bool IsRequired { get; set; }

        // Thông tin từ InspectionStage (nếu đã có)
        public long? InspStageId { get; set; }
        public int Status { get; set; } // 0: Pending, 1: InProgress, 2: Completed
        public int? StageResult { get; set; } // 1: Pass, 2: Fail, 3: Minor
        public Guid? AssignedUserId { get; set; }
        public string? AssignedUserName { get; set; }
        public string? Notes { get; set; }

        // Danh sách các item cần đo
        public List<StageItemDto> Items { get; set; } = new();
    }

    public class StageItemDto
    {
        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }
        public string? Unit { get; set; }
        public string? DataType { get; set; }
        public bool IsRequired { get; set; }
        public int SortOrder { get; set; }

        // Tiêu chuẩn
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string? PassCondition { get; set; }
        public string? AllowedValues { get; set; }

        // Giá trị đã đo
        public decimal? ActualValue { get; set; }
        public string? ActualText { get; set; }
        public bool? IsPassed { get; set; }

        public bool HasThreshold { get; set; }

    }

    public class SaveStageResultRequest
    {
        public int InspectionId { get; set; }
        public long InspStageId { get; set; }
        public int StageId { get; set; }
        public Guid UserId { get; set; }
        public List<StageItemMeasurement> Measurements { get; set; } = new();
        public string? Notes { get; set; }
    }


    public class StageItemMeasurement
    {
        public int ItemId { get; set; }
        public string? ItemCode { get; set; }
        public string? ItemName { get; set; }

        public decimal? StandardMin { get; set; }
        public decimal? StandardMax { get; set; }

        // ✅ THÊM TRƯỜNG ActualText
        public decimal? ActualValue { get; set; }
        public string? ActualText { get; set; }  // ✅ CHO AllowedValues

        public string? Unit { get; set; }
        public bool IsPassed { get; set; }

        // Defect info (nếu không đạt)
        public string? DefectCategory { get; set; }
        public string? DefectDescription { get; set; }
        public int? DefectSeverity { get; set; }
    }

    public class InspectionDefectDto
    {
        public long DefectId { get; set; }
        public int StageId { get; set; }
        public string DefectCategory { get; set; }
        public string DefectCode { get; set; }
        public string DefectDescription { get; set; }
        public int Severity { get; set; }
        public bool IsFixed { get; set; }
    }

    public class SubmitInspectionResultRequest
    {
        public int InspectionId { get; set; }
        public int? FinalResult { get; set; } // 1: ĐẠT, 2: KHÔNG ĐẠT, 3: TẠM ĐÌNH CHỈ
        public string? ConclusionNote { get; set; }
    }

    public class AssignLaneRequest
    {
        public int InspectionId { get; set; }
        public int LaneId { get; set; }
        public string? Note { get; set; }
    }
}
