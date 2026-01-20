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
        private JsonDocument? _cachedProvinceData;

        public ReceiveProfile(VehInsContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Lấy danh sách tỉnh/thành phố (chỉ tên)
        /// </summary>
        public async Task<List<string>> GetProvincesAsync()
        {
            try
            {
                if (_cachedProvinceData == null)
                {
                    await LoadProvinceJsonAsync();
                }

                if (_cachedProvinceData == null)
                {
                    return new List<string>();
                }

                var provinces = new List<string>();

                foreach (var element in _cachedProvinceData.RootElement.EnumerateArray())
                {
                    if (element.TryGetProperty("tentinhmoi", out var nameProperty))
                    {
                        var provinceName = nameProperty.GetString();
                        if (!string.IsNullOrWhiteSpace(provinceName))
                        {
                            provinces.Add(provinceName);
                        }
                    }
                }

                return provinces.OrderBy(p => p).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy danh sách tỉnh/thành phố: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Lấy danh sách phường/xã theo tỉnh
        /// </summary>
        public async Task<List<object>> GetWardsByProvinceAsync(string provinceName)
        {
            try
            {
                if (_cachedProvinceData == null)
                {
                    await LoadProvinceJsonAsync();
                }

                if (_cachedProvinceData == null || string.IsNullOrWhiteSpace(provinceName))
                {
                    return new List<object>();
                }

                var wards = new List<object>();

                foreach (var element in _cachedProvinceData.RootElement.EnumerateArray())
                {
                    if (element.TryGetProperty("tentinhmoi", out var nameProperty))
                    {
                        var currentProvinceName = nameProperty.GetString();

                        if (currentProvinceName?.Equals(provinceName, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            if (element.TryGetProperty("phuongxa", out var wardArray))
                            {
                                foreach (var wardElement in wardArray.EnumerateArray())
                                {
                                    if (wardElement.TryGetProperty("tenphuongxa", out var wardNameProp))
                                    {
                                        var wardName = wardNameProp.GetString();

                                        wards.Add(new
                                        {
                                            tenphuongxa = wardName,
                                            tenquanhuyen = ""
                                        });
                                    }
                                }
                            }
                            break;
                        }
                    }
                }

                return wards.OrderBy(w => ((dynamic)w).tenphuongxa).ToList();
            }
            catch (Exception ex)
            {
                throw new Exception($"Lỗi lấy danh sách phường/xã: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Load file JSON và cache
        /// </summary>
        private async Task LoadProvinceJsonAsync()
        {
            try
            {
                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Database", "SauXacNhap.json");

                Console.WriteLine($"🔍 Đang tìm file tại: {filePath}");

                if (!File.Exists(filePath))
                {
                    Console.WriteLine($"❌ File không tồn tại: {filePath}");
                    throw new FileNotFoundException("Không tìm thấy file dữ liệu địa phương");
                }

                var jsonString = await File.ReadAllTextAsync(filePath);
                Console.WriteLine($"✅ Đã đọc file JSON, độ dài: {jsonString.Length} ký tự");

                _cachedProvinceData = JsonDocument.Parse(jsonString);
                Console.WriteLine($"✅ Parse JSON thành công");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Lỗi load JSON: {ex.Message}");
                throw new Exception($"Lỗi load dữ liệu địa phương: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Tìm kiếm thông tin theo CCCD hoặc Biển số
        /// </summary>
        public async Task<SearchResponse?> SearchAsync(string? cccd, string? plateNo)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(cccd))
                {
                    var owner = await _context.Owners
                        .FirstOrDefaultAsync(o => o.CCCD == cccd);

                    if (owner != null)
                    {
                        // ✅ Include VehicleType để lấy TypeName
                        var vehicle = await _context.Vehicles
                            .Include(v => v.VehicleType)  // ✅ QUAN TRỌNG
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

                if (!string.IsNullOrWhiteSpace(plateNo))
                {
                    // ✅ Include VehicleType để lấy TypeName
                    var vehicle = await _context.Vehicles
                        .Include(v => v.VehicleType)  // ✅ QUAN TRỌNG
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
        /// Validate dữ liệu trước khi cập nhật
        /// </summary>
        public List<string> ValidateProfile(UpdateProfileRequest request)
        {
            var errors = new List<string>();

            // Validate Owner
            if (request.Owner == null)
            {
                errors.Add("Thông tin chủ xe không được để trống");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(request.Owner.FullName))
            {
                errors.Add("Họ và tên không được để trống");
            }

            // Validate CCCD nếu là cá nhân và có nhập CCCD
            if (request.Owner.OwnerType == "PERSON" && !string.IsNullOrWhiteSpace(request.Owner.CCCD))
            {
                if (!Regex.IsMatch(request.Owner.CCCD, @"^\d{9,12}$"))
                {
                    errors.Add("CCCD/CMND không hợp lệ (phải là 9-12 chữ số)");
                }
            }

            // Validate Vehicle
            if (request.Vehicle == null)
            {
                errors.Add("Thông tin xe không được để trống");
                return errors;
            }

            if (string.IsNullOrWhiteSpace(request.Vehicle.PlateNo))
            {
                errors.Add("Biển số xe không được để trống");
            }

            return errors;
        }

        /// <summary>
        /// Cập nhật thông tin Owner, Vehicle và Specification
        /// </summary>
        public async Task<bool> UpdateProfileAsync(UpdateProfileRequest request, string? imageUrl)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                Console.WriteLine($"💾 ========== BẮT ĐẦU CẬP NHẬT ==========");
                Console.WriteLine($"💾 Owner ID: {request.Owner.OwnerId}");
                Console.WriteLine($"📍 Province từ request: '{request.Owner.Province}'");
                Console.WriteLine($"📍 Ward từ request: '{request.Owner.Ward}'");

                // Tìm Owner
                var owner = await _context.Owners
                    .FirstOrDefaultAsync(o => o.OwnerId == request.Owner.OwnerId);

                if (owner == null)
                {
                    throw new Exception("Không tìm thấy thông tin chủ xe");
                }

                Console.WriteLine($"✅ Tìm thấy Owner: {owner.FullName}");
                Console.WriteLine($"   📍 Province CŨ: '{owner.Province ?? "NULL"}'");
                Console.WriteLine($"   📍 Ward CŨ: '{owner.Ward ?? "NULL"}'");

                // Cập nhật Owner
                owner.OwnerType = request.Owner.OwnerType;
                owner.FullName = request.Owner.FullName;
                owner.CompanyName = request.Owner.CompanyName;
                owner.TaxCode = request.Owner.TaxCode;
                owner.CCCD = request.Owner.CCCD;
                owner.Phone = request.Owner.Phone;
                owner.Email = request.Owner.Email;
                owner.Address = request.Owner.Address;

                if (!string.IsNullOrWhiteSpace(request.Owner.Province))
                {
                    owner.Province = request.Owner.Province;
                    Console.WriteLine($"   ✅ Đã gán Province: '{owner.Province}'");
                }
                else
                {
                    Console.WriteLine($"   ⚠️ Province request is empty!");
                }

                if (!string.IsNullOrWhiteSpace(request.Owner.Ward))
                {
                    owner.Ward = request.Owner.Ward;
                    Console.WriteLine($"   ✅ Đã gán Ward: '{owner.Ward}'");
                }
                else
                {
                    Console.WriteLine($"   ⚠️ Ward request is empty!");
                }

                Console.WriteLine($"   📍 Province SAU GÁN: '{owner.Province ?? "NULL"}'");
                Console.WriteLine($"   📍 Ward SAU GÁN: '{owner.Ward ?? "NULL"}'");

                if (!string.IsNullOrWhiteSpace(imageUrl))
                {
                    owner.ImageUrl = imageUrl;
                    Console.WriteLine($"📷 Cập nhật ảnh: {imageUrl}");
                }

                _context.Entry(owner).State = EntityState.Modified;
                Console.WriteLine($"✅ Entity state set to Modified");

                var modifiedProperties = _context.Entry(owner)
                    .Properties
                    .Where(p => p.IsModified)
                    .Select(p => p.Metadata.Name)
                    .ToList();
                Console.WriteLine($"📝 Modified properties: {string.Join(", ", modifiedProperties)}");

                // ✅ Cập nhật Vehicle
                var vehicle = await _context.Vehicles
                    .Include(v => v.VehicleType)  // ✅ Include để có thể log
                    .FirstOrDefaultAsync(v => v.VehicleId == request.Vehicle.VehicleId);

                if (vehicle == null)
                {
                    throw new Exception("Không tìm thấy thông tin xe");
                }

                Console.WriteLine($"✅ Tìm thấy Vehicle: {vehicle.PlateNo}");

                vehicle.PlateNo = request.Vehicle.PlateNo;
                vehicle.InspectionNo = request.Vehicle.InspectionNo;
                vehicle.VehicleGroup = request.Vehicle.VehicleGroup;

                // ✅ TÌM VehicleTypeId từ TypeName
                if (!string.IsNullOrWhiteSpace(request.Vehicle.VehicleType))
                {
                    var vehicleType = await _context.VehicleTypes
                        .FirstOrDefaultAsync(vt => vt.TypeName == request.Vehicle.VehicleType);

                    if (vehicleType != null)
                    {
                        vehicle.VehicleTypeId = vehicleType.VehicleTypeId;
                        Console.WriteLine($"✅ VehicleTypeId set to: {vehicle.VehicleTypeId} ({vehicleType.TypeName})");
                    }
                    else
                    {
                        Console.WriteLine($"⚠️ VehicleType '{request.Vehicle.VehicleType}' not found in database");
                    }
                }

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

                _context.Entry(vehicle).State = EntityState.Modified;

                // Cập nhật Specification
                if (request.Specification != null && request.Specification.SpecificationId > 0)
                {
                    var spec = await _context.Specifications
                        .FirstOrDefaultAsync(s => s.SpecificationId == request.Specification.SpecificationId);

                    if (spec != null)
                    {
                        UpdateSpecificationFields(spec, request.Specification);
                        _context.Entry(spec).State = EntityState.Modified;
                        Console.WriteLine($"✅ Đã cập nhật Specification");
                    }
                }

                // Lưu vào database
                Console.WriteLine($"💾 Đang gọi SaveChangesAsync...");

                try
                {
                    var savedCount = await _context.SaveChangesAsync();
                    Console.WriteLine($"💾 ✅ Đã lưu {savedCount} bản ghi vào database");
                }
                catch (DbUpdateException dbEx)
                {
                    Console.WriteLine($"❌ DbUpdateException: {dbEx.Message}");
                    Console.WriteLine($"❌ Inner: {dbEx.InnerException?.Message}");
                    throw;
                }

                // Verify
                var verifyOwner = await _context.Owners
                    .AsNoTracking()
                    .FirstOrDefaultAsync(o => o.OwnerId == request.Owner.OwnerId);

                if (verifyOwner != null)
                {
                    Console.WriteLine($"🔍 ========== VERIFY SAU KHI SAVE ==========");
                    Console.WriteLine($"   📍 Province trong DB: '{verifyOwner.Province ?? "NULL"}'");
                    Console.WriteLine($"   📍 Ward trong DB: '{verifyOwner.Ward ?? "NULL"}'");
                    Console.WriteLine($"🔍 ==========================================");
                }
                else
                {
                    Console.WriteLine($"❌ Không tìm thấy owner để verify!");
                }

                await transaction.CommitAsync();
                Console.WriteLine($"✅ Transaction committed successfully");
                Console.WriteLine($"💾 ========== KẾT THÚC CẬP NHẬT ==========");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ========== LỖI ==========");
                Console.WriteLine($"❌ Message: {ex.Message}");
                Console.WriteLine($"❌ Stack trace: {ex.StackTrace}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"❌ Inner Exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"❌ ===========================");

                await transaction.RollbackAsync();
                Console.WriteLine($"↩️ Transaction rolled back");

                throw new Exception($"Lỗi cập nhật: {ex.Message}", ex);
            }
        }
        /// <summary>
        /// Tạo mới hồ sơ Owner, Vehicle và Specification
        /// </summary>
        public async Task<bool> CreateProfileAsync(UpdateProfileRequest request, string? imageUrl)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                Console.WriteLine($"💾 ========== BẮT ĐẦU TẠO MỚI ==========");

                // Tạo Owner mới
                var owner = new Owner
                {
                    OwnerType = request.Owner.OwnerType,
                    FullName = request.Owner.FullName,
                    CompanyName = request.Owner.CompanyName,
                    TaxCode = request.Owner.TaxCode,
                    CCCD = request.Owner.CCCD,
                    Phone = request.Owner.Phone,
                    Email = request.Owner.Email,
                    Address = request.Owner.Address,
                    Ward = request.Owner.Ward,
                    Province = request.Owner.Province,
                    ImageUrl = imageUrl,
                    CreatedAt = DateTime.Now
                };

                _context.Owners.Add(owner);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Owner created with ID: {owner.OwnerId}");

                // Tạo Vehicle mới
                var vehicle = new Vehicle
                {
                    OwnerId = owner.OwnerId,
                    PlateNo = request.Vehicle.PlateNo,
                    InspectionNo = request.Vehicle.InspectionNo,
                    VehicleGroup = request.Vehicle.VehicleGroup,
                    EnergyType = request.Vehicle.EnergyType,
                    IsCleanEnergy = request.Vehicle.IsCleanEnergy,
                    UsagePermission = request.Vehicle.UsagePermission,
                    Brand = request.Vehicle.Brand,
                    Model = request.Vehicle.Model,
                    EngineNo = request.Vehicle.EngineNo,
                    Chassis = request.Vehicle.Chassis,
                    ManufactureYear = request.Vehicle.ManufactureYear,
                    ManufactureCountry = request.Vehicle.ManufactureCountry,
                    LifetimeLimitYear = request.Vehicle.LifetimeLimitYear,
                    HasCommercialModification = request.Vehicle.HasCommercialModification,
                    HasModification = request.Vehicle.HasModification,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };

                // Tìm VehicleTypeId
                if (!string.IsNullOrWhiteSpace(request.Vehicle.VehicleType))
                {
                    var vehicleType = await _context.VehicleTypes
                        .FirstOrDefaultAsync(vt => vt.TypeName == request.Vehicle.VehicleType);
                    if (vehicleType != null)
                    {
                        vehicle.VehicleTypeId = vehicleType.VehicleTypeId;
                    }
                }

                _context.Vehicles.Add(vehicle);
                await _context.SaveChangesAsync();

                Console.WriteLine($"✅ Vehicle created with ID: {vehicle.VehicleId}");

                // Tạo Specification nếu có
                if (request.Specification != null)
                {
                    var spec = new Specification
                    {
                        PlateNo = request.Vehicle.PlateNo,
                        WheelFormula = request.Specification.WheelFormula,
                        WheelTread = request.Specification.WheelTread,
                        OverallLength = request.Specification.OverallLength,
                        OverallWidth = request.Specification.OverallWidth,
                        OverallHeight = request.Specification.OverallHeight,
                        CargoInsideLength = request.Specification.CargoInsideLength,
                        CargoInsideWidth = request.Specification.CargoInsideWidth,
                        CargoInsideHeight = request.Specification.CargoInsideHeight,
                        Wheelbase = request.Specification.Wheelbase,
                        KerbWeight = request.Specification.KerbWeight,
                        AuthorizedCargoWeight = request.Specification.AuthorizedCargoWeight,
                        AuthorizedTowedWeight = request.Specification.AuthorizedTowedWeight,
                        AuthorizedTotalWeight = request.Specification.AuthorizedTotalWeight,
                        SeatingCapacity = request.Specification.SeatingCapacity,
                        StandingCapacity = request.Specification.StandingCapacity,
                        LyingCapacity = request.Specification.LyingCapacity,
                        EngineType = request.Specification.EngineType,
                        EnginePosition = request.Specification.EnginePosition,
                        EngineModel = request.Specification.EngineModel,
                        EngineDisplacement = request.Specification.EngineDisplacement,
                        MaxPower = request.Specification.MaxPower,
                        MaxPowerRPM = request.Specification.MaxPowerRPM,
                        FuelType = request.Specification.FuelType,
                        MotorType = request.Specification.MotorType,
                        NumberOfMotors = request.Specification.NumberOfMotors,
                        MotorModel = request.Specification.MotorModel,
                        TotalMotorPower = request.Specification.TotalMotorPower,
                        MotorVoltage = request.Specification.MotorVoltage,
                        BatteryType = request.Specification.BatteryType,
                        BatteryVoltage = request.Specification.BatteryVoltage,
                        BatteryCapacity = request.Specification.BatteryCapacity,
                        TireCount = request.Specification.TireCount,
                        TireSize = request.Specification.TireSize,
                        TireAxleInfo = request.Specification.TireAxleInfo,
                        ImagePosition = request.Specification.ImagePosition,
                        HasTachograph = request.Specification.HasTachograph,
                        HasDriverCamera = request.Specification.HasDriverCamera,
                        NotIssuedStamp = request.Specification.NotIssuedStamp,
                        Notes = request.Specification.Notes,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };

                    _context.Specifications.Add(spec);
                    await _context.SaveChangesAsync();

                    Console.WriteLine($"✅ Specification created");
                }

                await transaction.CommitAsync();
                Console.WriteLine($"✅ Transaction committed - Tạo mới thành công");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Create error: {ex.Message}");
                await transaction.RollbackAsync();
                throw new Exception($"Lỗi tạo mới: {ex.Message}", ex);
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
         
                VehicleType = vehicle.VehicleType?.TypeName ?? "",  
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
        public async Task<List<VehicleTypeDto>> GetVehicleTypesAsync()
        {
            try
            {
                var vehicleTypes = await _context.VehicleTypes
                    .Where(vt => vt.IsActive == true || vt.IsActive == null) // Lấy các loại đang active
                    .OrderBy(vt => vt.TypeName)
                    .Select(vt => new VehicleTypeDto
                    {
                        VehicleTypeId = vt.VehicleTypeId,
                        TypeCode = vt.TypeCode,
                        TypeName = vt.TypeName,
                        Description = vt.Description,
                        IsActive = vt.IsActive
                    })
                    .ToListAsync();

                Console.WriteLine($"✅ Loaded {vehicleTypes.Count} vehicle types");
                return vehicleTypes;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error loading vehicle types: {ex.Message}");
                throw new Exception($"Lỗi lấy danh sách loại phương tiện: {ex.Message}", ex);
            }
        }
    }
}