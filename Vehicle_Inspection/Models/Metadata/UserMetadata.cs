using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace Vehicle_Inspection.Models
{
    [ModelMetadataType(typeof(UserMetadata))]
    public partial class User
    {
    }

    public class UserMetadata
    {
        [Required(ErrorMessage = "Họ và tên không được để trống")]
        [StringLength(120, ErrorMessage = "Họ và tên không được vượt quá 120 ký tự")]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20, ErrorMessage = "Số điện thoại không được vượt quá 20 ký tự")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [StringLength(120, ErrorMessage = "Email không được vượt quá 120 ký tự")]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required(ErrorMessage = "CCCD không được để trống")]
        [StringLength(20, MinimumLength = 9, ErrorMessage = "CCCD phải từ 9-20 ký tự")]
        [Display(Name = "CCCD")]
        public string CCCD { get; set; }

        [Required(ErrorMessage = "Địa chỉ không được để trống")]
        [StringLength(255, ErrorMessage = "Địa chỉ không được vượt quá 255 ký tự")]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }

        [Required(ErrorMessage = "Giới tính không được để trống")]
        [StringLength(10, ErrorMessage = "Giới tính không được vượt quá 10 ký tự")]
        [Display(Name = "Giới tính")]
        public string Gender { get; set; }

        [Display(Name = "Cấp bậc")]
        [StringLength(50, ErrorMessage = "Cấp bậc không được vượt quá 50 ký tự")]
        public string? Level { get; set; }
    }
}