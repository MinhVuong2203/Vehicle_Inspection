using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Vehicle_Inspection.Models.Validation;

namespace Vehicle_Inspection.Models
{
    [ModelMetadataType(typeof(SpecificationMetadata))]
    [SpecificationValidation]
    public partial class Specification
    {
    }

    public class SpecificationMetadata
    {
        // --- PRIMARY KEY & FOREIGN KEY ---
        [Display(Name = "Mã thông số")]
        public int SpecificationId { get; set; }

        [Required(ErrorMessage = "Số biển xe không được để trống")]
        [Display(Name = "Số biển xe")]
        [StringLength(20, ErrorMessage = "Số biển xe không được quá 20 ký tự")]
        public string PlateNo { get; set; }

        // --- KÍCH THƯỚC - CÔNG THỨC BÁNH XE ---
        [Display(Name = "Công thức bánh xe")]
        [StringLength(50, ErrorMessage = "Công thức bánh xe không được quá 50 ký tự")]
        [RegularExpression(@"^(\d)x(\d)$", ErrorMessage = "Công thức bánh xe phải có dạng: 4x2, 6x4, ...")]
        public string? WheelFormula { get; set; }

        [Display(Name = "Vết bánh xe (mm)")]
        [Range(0, 9999, ErrorMessage = "Vết bánh xe phải từ 0 đến 9999 mm")]
        public int? WheelTread { get; set; }

        // --- KÍCH THƯỚC BAO (Overall Dimensions) ---
        [Display(Name = "Chiều dài (mm)")]
        [Range(1, 99999, ErrorMessage = "Chiều dài phải từ 1 đến 99999 mm")]
        public int? OverallLength { get; set; }

        [Display(Name = "Chiều rộng (mm)")]
        [Range(1, 99999, ErrorMessage = "Chiều rộng phải từ 1 đến 99999 mm")]
        public int? OverallWidth { get; set; }

        [Display(Name = "Chiều cao (mm)")]
        [Range(1, 99999, ErrorMessage = "Chiều cao phải từ 1 đến 99999 mm")]
        public int? OverallHeight { get; set; }

        // --- KÍCH THƯỚC LÒNG BÀO THÙNG XE ---
        [Display(Name = "Dài lòng thùng (mm)")]
        [Range(1, 99999, ErrorMessage = "Dài lòng thùng phải từ 1 đến 99999 mm")]
        public int? CargoInsideLength { get; set; }

        [Display(Name = "Rộng lòng thùng (mm)")]
        [Range(1, 99999, ErrorMessage = "Rộng lòng thùng phải từ 1 đến 99999 mm")]
        public int? CargoInsideWidth { get; set; }

        [Display(Name = "Cao lòng thùng (mm)")]
        [Range(1, 99999, ErrorMessage = "Cao lòng thùng phải từ 1 đến 99999 mm")]
        public int? CargoInsideHeight { get; set; }

        // --- KHOẢNG CÁCH TRỤC ---
        [Display(Name = "Khoảng cách trục (mm)")]
        [Range(1, 99999, ErrorMessage = "Khoảng cách trục phải từ 1 đến 99999 mm")]
        public int? Wheelbase { get; set; }

        // --- KHỐI LƯỢNG ---
        [Display(Name = "Khối lượng bản thân (kg)")]
        [Range(0.01, 99999.99, ErrorMessage = "Khối lượng bản thân phải từ 0.01 đến 99999.99 kg")]
        public decimal? KerbWeight { get; set; }

        [Display(Name = "Khối lượng hàng CC theo TK/CP-N (kg)")]
        [Range(0.01, 99999.99, ErrorMessage = "Khối lượng hàng CC phải từ 0.01 đến 99999.99 kg")]
        public decimal? AuthorizedCargoWeight { get; set; }

        [Display(Name = "Khối lượng kéo theo TK/CP-N (kg)")]
        [Range(0.01, 99999.99, ErrorMessage = "Khối lượng kéo phải từ 0.01 đến 99999.99 kg")]
        public decimal? AuthorizedTowedWeight { get; set; }

        [Display(Name = "Khối lượng toàn bộ theo TK/CP-N (kg)")]
        [Range(0.01, 999999.99, ErrorMessage = "Khối lượng toàn bộ phải từ 0.01 đến 999999.99 kg")]
        public decimal? AuthorizedTotalWeight { get; set; }

        // --- SỐ NGƯỜI CHO PHÉP CHỞ ---
        [Display(Name = "Chở ngồi (người)")]
        [Range(0, 999, ErrorMessage = "Số người ngồi phải từ 0 đến 999")]
        public int? SeatingCapacity { get; set; }

        [Display(Name = "Chở đứng (người)")]
        [Range(0, 999, ErrorMessage = "Số người đứng phải từ 0 đến 999")]
        public int? StandingCapacity { get; set; }

        [Display(Name = "Chở nằm (người)")]
        [Range(0, 999, ErrorMessage = "Số người nằm phải từ 0 đến 999")]
        public int? LyingCapacity { get; set; }

        // --- ĐỘNG CƠ (ENGINE) ---
        [Display(Name = "Loại động cơ")]
        [StringLength(100, ErrorMessage = "Loại động cơ không được quá 100 ký tự")]
        public string? EngineType { get; set; }

        [Display(Name = "Vị trí đặt động cơ")]
        [StringLength(50, ErrorMessage = "Vị trí đặt động cơ không được quá 50 ký tự")]
        public string? EnginePosition { get; set; }

        [Display(Name = "Ký hiệu động cơ")]
        [StringLength(50, ErrorMessage = "Ký hiệu động cơ không được quá 50 ký tự")]
        public string? EngineModel { get; set; }

        [Display(Name = "Thể tích làm việc (cm³)")]
        [Range(1, 99999, ErrorMessage = "Thể tích làm việc phải từ 1 đến 99999 cm³")]
        public int? EngineDisplacement { get; set; }

        [Display(Name = "Công suất lớn nhất (kW)")]
        [Range(0.01, 99999.99, ErrorMessage = "Công suất lớn nhất phải từ 0.01 đến 99999.99 kW")]
        public decimal? MaxPower { get; set; }

        [Display(Name = "Tốc độ quay tại công suất max (rpm)")]
        [Range(1, 99999, ErrorMessage = "Tốc độ quay phải từ 1 đến 99999 rpm")]
        public int? MaxPowerRPM { get; set; }

        [Display(Name = "Loại nhiên liệu")]
        [StringLength(50, ErrorMessage = "Loại nhiên liệu không được quá 50 ký tự")]
        public string? FuelType { get; set; }

        // --- ĐỘNG CƠ ĐIỆN (MOTOR) ---
        [Display(Name = "Loại động cơ điện")]
        [StringLength(100, ErrorMessage = "Loại động cơ điện không được quá 100 ký tự")]
        public string? MotorType { get; set; }

        [Display(Name = "Số lượng động cơ điện")]
        [Range(0, 99, ErrorMessage = "Số lượng động cơ điện phải từ 0 đến 99")]
        public int? NumberOfMotors { get; set; }

        [Display(Name = "Ký hiệu động cơ điện")]
        [StringLength(50, ErrorMessage = "Ký hiệu động cơ điện không được quá 50 ký tự")]
        public string? MotorModel { get; set; }

        [Display(Name = "Tổng công suất (kW)")]
        [Range(0.01, 99999.99, ErrorMessage = "Tổng công suất phải từ 0.01 đến 99999.99 kW")]
        public decimal? TotalMotorPower { get; set; }

        [Display(Name = "Điện áp động cơ (V)")]
        [Range(0.01, 9999.99, ErrorMessage = "Điện áp động cơ phải từ 0.01 đến 9999.99 V")]
        public decimal? MotorVoltage { get; set; }

        // --- ẮC QUY (BATTERY) ---
        [Display(Name = "Loại ắc quy")]
        [StringLength(100, ErrorMessage = "Loại ắc quy không được quá 100 ký tự")]
        public string? BatteryType { get; set; }

        [Display(Name = "Điện áp ắc quy (V)")]
        [Range(0.01, 9999.99, ErrorMessage = "Điện áp ắc quy phải từ 0.01 đến 9999.99 V")]
        public decimal? BatteryVoltage { get; set; }

        [Display(Name = "Dung lượng ắc quy (kWh)")]
        [Range(0.01, 9999.99, ErrorMessage = "Dung lượng ắc quy phải từ 0.01 đến 9999.99 kWh")]
        public decimal? BatteryCapacity { get; set; }

        // --- LỐP XE (TIRES) ---
        [Display(Name = "Số lượng lốp")]
        [Range(0, 99, ErrorMessage = "Số lượng lốp phải từ 0 đến 99")]
        public int? TireCount { get; set; }

        [Display(Name = "Cỡ lốp")]
        [StringLength(50, ErrorMessage = "Cỡ lốp không được quá 50 ký tự")]
        public string? TireSize { get; set; }

        [Display(Name = "Thông tin trục")]
        [StringLength(100, ErrorMessage = "Thông tin trục không được quá 100 ký tự")]
        public string? TireAxleInfo { get; set; }

        // --- VỊ TRÍ THIẾT BỊ ---
        [Display(Name = "Vị trí hình ảnh")]
        [StringLength(100, ErrorMessage = "Vị trí hình ảnh không được quá 100 ký tự")]
        public string? ImagePosition { get; set; }

        // --- TRANG THIẾT BỊ ---
        [Display(Name = "Có thiết bị giám sát hành trình")]
        public bool? HasTachograph { get; set; }

        [Display(Name = "Có camera ghi nhận lái xe")]
        public bool? HasDriverCamera { get; set; }

        [Display(Name = "PT không được cấp tem")]
        public bool? NotIssuedStamp { get; set; }

        // --- GHI CHÚ ---
        [Display(Name = "Ghi chú")]
        [StringLength(1000, ErrorMessage = "Ghi chú không được quá 1000 ký tự")]
        public string? Notes { get; set; }

        // --- TIMESTAMPS ---
        [Display(Name = "Ngày tạo")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Ngày cập nhật")]
        [DataType(DataType.DateTime)]
        public DateTime? UpdatedAt { get; set; }

        [Display(Name = "Người tạo")]
        public Guid? CreatedBy { get; set; }

        [Display(Name = "Người cập nhật")]
        public Guid? UpdatedBy { get; set; }
    }
}