using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using Vehicle_Inspection.Models.Validation;

namespace Vehicle_Inspection.Models.Metadata
{
    [ModelMetadataType(typeof(OwnerMetadata))]
    [OwnerValidation]
    public partial class Owner
    {
    }

    public class OwnerMetadata
    {
        [Display(Name = "Mã chủ xe")]
        public Guid OwnerId { get; set; }

        [Required(ErrorMessage = "Loại chủ xe không được để trống")]
        [Display(Name = "Loại chủ xe")]
        [RegularExpression("^(PERSON|COMPANY)$", ErrorMessage = "Loại chủ xe chỉ được là PERSON hoặc COMPANY")]
        public string OwnerType { get; set; }

        [Required(ErrorMessage = "Họ và tên không được để trống")]
        [Display(Name = "Họ và tên")]
        [StringLength(150, MinimumLength = 2, ErrorMessage = "Họ và tên phải từ 2-150 ký tự")]
        [RegularExpression(@"^[\p{L}\s]+$", ErrorMessage = "Họ và tên chỉ được chứa chữ cái và khoảng trắng")]
        public string FullName { get; set; }

        [Display(Name = "Tên công ty")]
        [StringLength(200, ErrorMessage = "Tên công ty không được quá 200 ký tự")]
        public string? CompanyName { get; set; }

        [Display(Name = "Mã số thuế")]
        [StringLength(30, ErrorMessage = "Mã số thuế không được quá 30 ký tự")]
        [RegularExpression(@"^\d{10}(-\d{3})?$", ErrorMessage = "Mã số thuế phải có định dạng: 0123456789 hoặc 0123456789-001")]
        public string? TaxCode { get; set; }

        [Display(Name = "CCCD/CMND")]
        [StringLength(30, ErrorMessage = "CCCD/CMND không được quá 30 ký tự")]
        [RegularExpression(@"^\d{9}(\d{3})?$", ErrorMessage = "CCCD/CMND phải là 9 hoặc 12 chữ số")]
        public string? CCCD { get; set; }

        [Display(Name = "Số điện thoại")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được quá 20 ký tự")]
        [RegularExpression(@"^(0|\+84)[3|5|7|8|9]\d{8}$", ErrorMessage = "Số điện thoại không hợp lệ (VD: 0912345678 hoặc +84912345678)")]
        public string? Phone { get; set; }

        [Display(Name = "Email")]
        [StringLength(120, ErrorMessage = "Email không được quá 120 ký tự")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string? Email { get; set; }

        [Display(Name = "Địa chỉ")]
        [StringLength(255, ErrorMessage = "Địa chỉ không được quá 255 ký tự")]
        public string? Address { get; set; }

        [Display(Name = "Ngày tạo")]
        [DataType(DataType.DateTime)]
        public DateTime CreatedAt { get; set; }

        [Display(Name = "Phường/Xã")]
        [StringLength(100, ErrorMessage = "Phường/Xã không được quá 100 ký tự")]
        public string? Ward { get; set; }

        [Display(Name = "Tỉnh/Thành phố")]
        [StringLength(100, ErrorMessage = "Tỉnh/Thành phố không được quá 100 ký tự")]
        public string? Province { get; set; }

        [Display(Name = "Ảnh đại diện")]
        [StringLength(255, ErrorMessage = "URL ảnh không được quá 255 ký tự")]
        [Url(ErrorMessage = "URL ảnh không hợp lệ")]
        public string? ImageUrl { get; set; }
    }
}