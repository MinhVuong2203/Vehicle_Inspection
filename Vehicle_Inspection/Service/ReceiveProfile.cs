using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.RegularExpressions;
using Vehicle_Inspection.Data;
using Vehicle_Inspection.Models;
using static Vehicle_Inspection.Service.IReceiveProfile;

namespace Vehicle_Inspection.Service
{
    public class ReceiveProfile : IReceiveProfile
    {
        private readonly VehInsContext _context;

        public ReceiveProfile(VehInsContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Tìm kiếm thống nhất - ưu tiên CCCD trước, sau đó PlateNo
        /// </summary>
        public async Task<SearchResponse?> SearchAsync(string? cccd, string? plateNo)
        {
            try
            {
                // Ưu tiên tìm theo CCCD
                if (!string.IsNullOrWhiteSpace(cccd))
                {
                    var owner = await _context.Owners
                        .FirstOrDefaultAsync(o => o.CCCD == cccd);

                    if (owner != null)
                    {
                        var vehicle = await _context.Vehicles
                            .FirstOrDefaultAsync(v => v.OwnerId == owner.OwnerId);

                        if (vehicle != null)
                        {
                            var specification = await _context.Specifications
                                .FirstOrDefaultAsync(s => s.PlateNo == vehicle.PlateNo);

                            return new SearchResponse
                            {
                                SearchType = "cccd",
                                Data = new SearchResultDto
                                {
                                    Owner = MapToOwnerDto(owner),
                                    Vehicle = MapToVehicleDto(vehicle),
                                    Specification = specification != null ? MapToSpecificationDto(specification) : null
                                }
                            };
                        }
                    }
                }

                // Nếu không tìm thấy theo CCCD, thử PlateNo
                if (!string.IsNullOrWhiteSpace(plateNo))
                {
                    var vehicle = await _context.Vehicles
                        .FirstOrDefaultAsync(v => v.PlateNo == plateNo);

                    if (vehicle != null)
                    {
                        var owner = await _context.Owners
                            .FirstOrDefaultAsync(o => o.OwnerId == vehicle.OwnerId);

                        if (owner != null)
                        {
                            var specification = await _context.Specifications
                                .FirstOrDefaultAsync(s => s.PlateNo == vehicle.PlateNo);

                            return new SearchResponse
                            {
                                SearchType = "plateNo",
                                Data = new SearchResultDto
                                {
                                    Owner = MapToOwnerDto(owner),
                                    Vehicle = MapToVehicleDto(vehicle),
                                    Specification = specification != null ? MapToSpecificationDto(specification) : null
                                }
                            };
                        }
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi tìm kiếm: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Validate dữ liệu profile trên server
        /// </summary>
        public List<string> ValidateProfile(UpdateProfileRequest request)
        {
            var errors = new List<string>();

            // Owner validation
            if (string.IsNullOrWhiteSpace(request.Owner.FullName))
            {
                errors.Add("Họ và tên không được để trống");
            }

            if (request.Owner.OwnerType == "PERSON")
            {
                if (!string.IsNullOrWhiteSpace(request.Owner.CCCD))
                {
                    if (!Regex.IsMatch(request.Owner.CCCD, @"^\d{9,12}$"))
                    {
                        errors.Add("CCCD/CMND không hợp lệ (9-12 số)");
                    }
                }
            }

            // Vehicle validation
            if (string.IsNullOrWhiteSpace(request.Vehicle.PlateNo))
            {
                errors.Add("Biển số xe không được để trống");
            }

            return errors;
        }

        /// <summary>
        /// Cập nhật thông tin Owner, Vehicle và Specification
        /// </summary>
        public async Task<bool> UpdateProfileAsync(UpdateProfileRequest request)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                // Update Owner
                var owner = await _context.Owners
                    .FirstOrDefaultAsync(o => o.OwnerId == request.Owner.OwnerId);

                if (owner != null)
                {
                    owner.OwnerType = request.Owner.OwnerType;
                    owner.FullName = request.Owner.FullName;
                    owner.CompanyName = request.Owner.CompanyName;
                    owner.TaxCode = request.Owner.TaxCode;
                    owner.CCCD = request.Owner.CCCD;
                    owner.Phone = request.Owner.Phone;
                    owner.Email = request.Owner.Email;
                    owner.Address = request.Owner.Address;
                    owner.Ward = request.Owner.Ward;
                    owner.Province = request.Owner.Province;

                    _context.Owners.Update(owner);
                }

                // Update Vehicle
                var vehicle = await _context.Vehicles
                    .FirstOrDefaultAsync(v => v.VehicleId == request.Vehicle.VehicleId);

                if (vehicle != null)
                {
                    vehicle.PlateNo = request.Vehicle.PlateNo;
                    vehicle.InspectionNo = request.Vehicle.InspectionNo;
                    vehicle.VehicleGroup = request.Vehicle.VehicleGroup;
                    vehicle.VehicleType = request.Vehicle.VehicleType;
                    vehicle.EnergyType = request.Vehicle.EnergyType;
                    vehicle.IsCleanEnergy = request.Vehicle.IsCleanEnergy;
                    vehicle.UsagePermission = request.Vehicle.UsagePermission;
                    vehicle.Brand = request.Vehicle.Brand;
                    vehicle.Model = request.Vehicle.Model;
                    vehicle.EngineNo = request.Vehicle.EngineNo;
                    vehicle.Chassis = request.Vehicle.Chassis;
                    vehicle.ManufactureYear = request.Vehicle.ManufactureYear;
                    vehicle.ManufactureCountry = request.Vehicle.ManufactureCountry;
                    vehicle.LifetimeLimitYear = request.Vehicle.LifetimeLimitYear;
                    vehicle.HasCommercialModification = request.Vehicle.HasCommercialModification;
                    vehicle.HasModification = request.Vehicle.HasModification;
                    vehicle.UpdatedAt = DateTime.Now;

                    _context.Vehicles.Update(vehicle);
                }

                // Update Specification
                if (request.Specification != null)
                {
                    var spec = await _context.Specifications
                        .FirstOrDefaultAsync(s => s.SpecificationId == request.Specification.SpecificationId);

                    if (spec != null)
                    {
                        UpdateSpecificationFields(spec, request.Specification);
                        _context.Specifications.Update(spec);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return true;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                throw new Exception($"Lỗi cập nhật: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lấy dữ liệu tỉnh/thành phố từ file JSON
        /// </summary>
        public async Task<object?> GetProvincesAsync()
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Database", "SauXacNhap.json");

                if (!File.Exists(filePath))
                {
                    return null;
                }

                var jsonData = await File.ReadAllTextAsync(filePath);
                var provinces = JsonSerializer.Deserialize<object>(jsonData);

                return provinces;
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi đọc file tỉnh/thành phố: {ex.Message}", ex);
            }
        }

        // ==================== PRIVATE HELPER METHODS ====================

        private void UpdateSpecificationFields(Specification spec, SpecificationDto dto)
        {
            spec.WheelFormula = dto.WheelFormula;
            spec.WheelTread = dto.WheelTread;
            spec.OverallLength = dto.OverallLength;
            spec.OverallWidth = dto.OverallWidth;
            spec.OverallHeight = dto.OverallHeight;
            spec.CargoInsideLength = dto.CargoInsideLength;
            spec.CargoInsideWidth = dto.CargoInsideWidth;
            spec.CargoInsideHeight = dto.CargoInsideHeight;
            spec.Wheelbase = dto.Wheelbase;
            spec.KerbWeight = dto.KerbWeight;
            spec.AuthorizedCargoWeight = dto.AuthorizedCargoWeight;
            spec.AuthorizedTowedWeight = dto.AuthorizedTowedWeight;
            spec.AuthorizedTotalWeight = dto.AuthorizedTotalWeight;
            spec.SeatingCapacity = dto.SeatingCapacity;
            spec.StandingCapacity = dto.StandingCapacity;
            spec.LyingCapacity = dto.LyingCapacity;
            spec.EngineType = dto.EngineType;
            spec.EnginePosition = dto.EnginePosition;
            spec.EngineModel = dto.EngineModel;
            spec.EngineDisplacement = dto.EngineDisplacement;
            spec.MaxPower = dto.MaxPower;
            spec.MaxPowerRPM = dto.MaxPowerRPM;
            spec.FuelType = dto.FuelType;
            spec.MotorType = dto.MotorType;
            spec.NumberOfMotors = dto.NumberOfMotors;
            spec.MotorModel = dto.MotorModel;
            spec.TotalMotorPower = dto.TotalMotorPower;
            spec.MotorVoltage = dto.MotorVoltage;
            spec.BatteryType = dto.BatteryType;
            spec.BatteryVoltage = dto.BatteryVoltage;
            spec.BatteryCapacity = dto.BatteryCapacity;
            spec.TireCount = dto.TireCount;
            spec.TireSize = dto.TireSize;
            spec.TireAxleInfo = dto.TireAxleInfo;
            spec.ImagePosition = dto.ImagePosition;
            spec.HasTachograph = dto.HasTachograph;
            spec.HasDriverCamera = dto.HasDriverCamera;
            spec.NotIssuedStamp = dto.NotIssuedStamp;
            spec.Notes = dto.Notes;
            spec.UpdatedAt = DateTime.Now;
        }

        private OwnerDto MapToOwnerDto(Owner owner)
        {
            return new OwnerDto
            {
                OwnerId = owner.OwnerId,
                OwnerType = owner.OwnerType,
                FullName = owner.FullName,
                CompanyName = owner.CompanyName,
                TaxCode = owner.TaxCode,
                CCCD = owner.CCCD,
                Phone = owner.Phone,
                Email = owner.Email,
                Address = owner.Address,
                Ward = owner.Ward,
                Province = owner.Province,
                ImageUrl = owner.ImageUrl,
                CreatedAt = owner.CreatedAt
            };
        }

        private VehicleDto MapToVehicleDto(Vehicle vehicle)
        {
            return new VehicleDto
            {
                VehicleId = vehicle.VehicleId,
                PlateNo = vehicle.PlateNo,
                InspectionNo = vehicle.InspectionNo,
                VehicleGroup = vehicle.VehicleGroup,
                VehicleType = vehicle.VehicleType,
                EnergyType = vehicle.EnergyType,
                IsCleanEnergy = vehicle.IsCleanEnergy,
                UsagePermission = vehicle.UsagePermission,
                Brand = vehicle.Brand,
                Model = vehicle.Model,
                EngineNo = vehicle.EngineNo,
                Chassis = vehicle.Chassis,
                ManufactureYear = vehicle.ManufactureYear,
                ManufactureCountry = vehicle.ManufactureCountry,
                LifetimeLimitYear = vehicle.LifetimeLimitYear,
                HasCommercialModification = vehicle.HasCommercialModification,
                HasModification = vehicle.HasModification,
                CreatedAt = vehicle.CreatedAt,
                UpdatedAt = vehicle.UpdatedAt
            };
        }

        private SpecificationDto MapToSpecificationDto(Specification spec)
        {
            return new SpecificationDto
            {
                SpecificationId = spec.SpecificationId,
                PlateNo = spec.PlateNo,
                WheelFormula = spec.WheelFormula,
                WheelTread = spec.WheelTread,
                OverallLength = spec.OverallLength,
                OverallWidth = spec.OverallWidth,
                OverallHeight = spec.OverallHeight,
                CargoInsideLength = spec.CargoInsideLength,
                CargoInsideWidth = spec.CargoInsideWidth,
                CargoInsideHeight = spec.CargoInsideHeight,
                Wheelbase = spec.Wheelbase,
                KerbWeight = spec.KerbWeight,
                AuthorizedCargoWeight = spec.AuthorizedCargoWeight,
                AuthorizedTowedWeight = spec.AuthorizedTowedWeight,
                AuthorizedTotalWeight = spec.AuthorizedTotalWeight,
                SeatingCapacity = spec.SeatingCapacity,
                StandingCapacity = spec.StandingCapacity,
                LyingCapacity = spec.LyingCapacity,
                EngineType = spec.EngineType,
                EnginePosition = spec.EnginePosition,
                EngineModel = spec.EngineModel,
                EngineDisplacement = spec.EngineDisplacement,
                MaxPower = spec.MaxPower,
                MaxPowerRPM = spec.MaxPowerRPM,
                FuelType = spec.FuelType,
                MotorType = spec.MotorType,
                NumberOfMotors = spec.NumberOfMotors,
                MotorModel = spec.MotorModel,
                TotalMotorPower = spec.TotalMotorPower,
                MotorVoltage = spec.MotorVoltage,
                BatteryType = spec.BatteryType,
                BatteryVoltage = spec.BatteryVoltage,
                BatteryCapacity = spec.BatteryCapacity,
                TireCount = spec.TireCount,
                TireSize = spec.TireSize,
                TireAxleInfo = spec.TireAxleInfo,
                ImagePosition = spec.ImagePosition,
                HasTachograph = spec.HasTachograph,
                HasDriverCamera = spec.HasDriverCamera,
                NotIssuedStamp = spec.NotIssuedStamp,
                Notes = spec.Notes,
                CreatedAt = spec.CreatedAt,
                UpdatedAt = spec.UpdatedAt
            };
        }
    }
}