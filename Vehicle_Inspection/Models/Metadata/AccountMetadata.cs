using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Vehicle_Inspection.Models.NewFolder
{

    [MetadataType(typeof(AccountMetadata))]
    public partial class Account
    {
        // Không cần viết gì ở đây
    }

    // Class chứa các validation rules
    public class AccountMetadata
    {
        [Display(Name = "ID Người dùng")]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Username không được để trống")]
        [StringLength(50, MinimumLength = 6, ErrorMessage = "Username phải từ 6-50 ký tự")]
        [RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])[A-Za-z0-9_]+$",
            ErrorMessage = "Username phải có chữ hoa, chữ thường, chỉ chứa chữ cái, số và dấu _")]
        [Remote(action: "CheckUsername", controller: "Account",
            AdditionalFields = "UserId", ErrorMessage = "Username đã tồn tại")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu không được để trống")]
        [StringLength(255, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8-255 ký tự")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string PasswordHash { get; set; } = null!;

        [Display(Name = "Tài khoản bị khóa")]
        public bool IsLocked { get; set; }

        [Display(Name = "Số lần đăng nhập thất bại")]
        [Range(0, int.MaxValue, ErrorMessage = "Số lần thất bại phải >= 0")]
        public int FailedCount { get; set; }

        [Display(Name = "Lần đăng nhập cuối")]
        [DataType(DataType.DateTime)]
        public DateTime? LastLoginAt { get; set; }

        // Navigation property - không cần validation
        public virtual User User { get; set; } = null!;
    }
}
