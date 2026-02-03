using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Vehicle_Inspection.Models.Validation;

namespace Vehicle_Inspection.Models.Metadata
{
    [ModelMetadataType(typeof(VehicleMetadata))]
    [VehicleValidation]
    public partial class Vehicle
    {

    }
    public class VehicleMetadata
    {
        [Display(Name = "Mã phương tiện")]
        public int VehicleId { get; set; }

        [Required(ErrorMessage = "Biển số xe không được để trống")]
        [Display(Name = "Biển số xe")]
        [StringLength(20, ErrorMessage = "Biển số xe không được quá 20 ký tự")]
        [RegularExpression(@"^[0-9]{2}[A-Z]{1,2}-[0-9]{4,5}$", ErrorMessage = "Biển số xe không đúng định dạng (VD: 51A-12345)")]
        public string PlateNo { get; set; }

        [Display(Name = "Số quản lý")]
        [StringLength(50, ErrorMessage = "Số quản lý không được quá 50 ký tự")]
        public string? InspectionNo { get; set; }

        [Display(Name = "Nhóm phương tiện")]
        [StringLength(100, ErrorMessage = "Nhóm phương tiện không được quá 100 ký tự")]
        public string? VehicleGroup { get; set; }

        [Display(Name = "Loại năng lượng")]
        [StringLength(50, ErrorMessage = "Loại năng lượng không được quá 50 ký tự")]
        public string? EnergyType { get; set; }

        [Display(Name = "Năng lượng sạch")]
        public bool? IsCleanEnergy { get; set; }

        [Display(Name = "Quyền sử dụng")]
        [StringLength(20, ErrorMessage = "Quyền sử dụng không được quá 20 ký tự")]
        [RegularExpression("^(Một phần|Toàn phần)?$", ErrorMessage = "Quyền sử dụng chỉ được là 'Một phần' hoặc 'Toàn phần'")]
        public string? UsagePermission { get; set; }

        [Display(Name = "Nhãn hiệu")]
        [StringLength(100, ErrorMessage = "Nhãn hiệu không được quá 100 ký tự")]
        public string? Brand { get; set; }

        [Display(Name = "Mã kiểu loại")]
        [StringLength(100, ErrorMessage = "Mã kiểu loại không được quá 100 ký tự")]
        public string? Model { get; set; }

        [Display(Name = "Số động cơ")]
        [StringLength(50, ErrorMessage = "Số động cơ không được quá 50 ký tự")]
        public string? EngineNo { get; set; }

        [Display(Name = "Số khung")]
        [StringLength(50, ErrorMessage = "Số khung không được quá 50 ký tự")]
        public string? Chassis { get; set; }

        [Display(Name = "Năm sản xuất")]
        [Range(1900, 2100, ErrorMessage = "Năm sản xuất phải từ 1900 đến 2100")]
        public int? ManufactureYear { get; set; }

        [Display(Name = "Nước sản xuất")]
        [StringLength(100, ErrorMessage = "Nước sản xuất không được quá 100 ký tự")]
        public string? ManufactureCountry { get; set; }

        [Display(Name = "Niên hạn sử dụng")]
        [Range(1, 100, ErrorMessage = "Niên hạn sử dụng phải từ 1 đến 100 năm")]
        public int? LifetimeLimitYear { get; set; }

        [Display(Name = "Kinh doanh vận tải")]
        public bool? HasCommercialModification { get; set; }

        [Display(Name = "Đã cải tạo")]
        public bool? HasModification { get; set; }

        [Required(ErrorMessage = "Chủ xe không được để trống")]
        [Display(Name = "Chủ xe")]
        public Guid OwnerId { get; set; }

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

        [Display(Name = "Loại phương tiện")]
        public int? VehicleTypeId { get; set; }
    }
}
