using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace Vehicle_Inspection.Models
{
    [ModelMetadataType(typeof(AccountMetadata))]
    public partial class Account
    {
    }

    public class AccountMetadata
    {
        [RegularExpression(@"^(?=.{6,50}$)(?=.*[A-Z])(?=.*[a-z])[A-Za-z0-9_]+$",
        ErrorMessage = "Tên đăng nhập phải từ 6-50 ký tự, có chữ hoa, chữ thường, chỉ chứa chữ cái, số và dấu _")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }

        [RegularExpression(@"^(?=.{8,255}$)(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^\w\s])\S+$",
        ErrorMessage = "Mật khẩu phải từ 8-255 ký tự, có chữ hoa, chữ thường, số và ít nhất 1 ký tự đặc biệt, không chứa khoảng trắng")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string PasswordHash { get; set; }

    }
}