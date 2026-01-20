using Vehicle_Inspection.Models;

namespace Vehicle_Inspection.Service
{
    public interface IReceiveProfile
    {
        Task<SearchResponse?> SearchAsync(string? cccd, string? plateNo);
        List<string> ValidateProfile(UpdateProfileRequest request);
        Task<bool> UpdateProfileAsync(UpdateProfileRequest request, string? imageUrl);
        Task<List<string>> GetProvincesAsync();
        Task<List<object>> GetWardsByProvinceAsync(string provinceName);
        Task<bool> CreateProfileAsync(UpdateProfileRequest request, string? imageUrl);
        Task<List<VehicleTypeDto>> GetVehicleTypesAsync();

        // DTOs
        public class SearchResponse
        {
            public string SearchType { get; set; }
            public SearchResultDto Data { get; set; }
        }

        public class SearchResultDto
        {
            public OwnerDto Owner { get; set; }
            public VehicleDto Vehicle { get; set; }
            public SpecificationDto? Specification { get; set; }
        }

        public class OwnerDto
        {
            public Guid OwnerId { get; set; }
            public string OwnerType { get; set; }
            public string FullName { get; set; }
            public string? CompanyName { get; set; }
            public string? TaxCode { get; set; }
            public string? CCCD { get; set; }
            public string? Phone { get; set; }
            public string? Email { get; set; }
            public string? Address { get; set; }
            public string? Ward { get; set; }
            public string? Province { get; set; }
            public string? ImageUrl { get; set; }
            public DateTime CreatedAt { get; set; }
        }

        public class VehicleDto
        {
            public int VehicleId { get; set; }
            public string PlateNo { get; set; }
            public string? InspectionNo { get; set; }
            public string? VehicleGroup { get; set; }
            public string? VehicleType { get; set; }
            public string? EnergyType { get; set; }
            public bool? IsCleanEnergy { get; set; }
            public string? UsagePermission { get; set; }
            public string? Brand { get; set; }
            public string? Model { get; set; }
            public string? EngineNo { get; set; }
            public string? Chassis { get; set; }
            public int? ManufactureYear { get; set; }
            public string? ManufactureCountry { get; set; }
            public int? LifetimeLimitYear { get; set; }
            public bool? HasCommercialModification { get; set; }
            public bool? HasModification { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }

        public class SpecificationDto
        {
            public int SpecificationId { get; set; }
            public string PlateNo { get; set; }
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
            public string? ImagePosition { get; set; }
            public bool? HasTachograph { get; set; }
            public bool? HasDriverCamera { get; set; }
            public bool? NotIssuedStamp { get; set; }
            public string? Notes { get; set; }
            public DateTime CreatedAt { get; set; }
            public DateTime? UpdatedAt { get; set; }
        }

        public class UpdateProfileRequest
        {
            public OwnerDto Owner { get; set; }
            public VehicleDto Vehicle { get; set; }
            public SpecificationDto? Specification { get; set; }
        }
        public class VehicleTypeDto
        {
            public int VehicleTypeId { get; set; }
            public string TypeCode { get; set; } = null!;
            public string TypeName { get; set; } = null!;
            public string? Description { get; set; }
            public bool? IsActive { get; set; }
        }
    }
}