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
        //[StringLength(50, MinimumLength = 6, ErrorMessage = "Tên đăng nhập phải từ 6-50 ký tự")]
        //[RegularExpression(@"^(?=.*[A-Z])(?=.*[a-z])[A-Za-z0-9_]+$",
        //    ErrorMessage = "Tên đăng nhập phải có chữ hoa, chữ thường, chỉ chứa chữ cái, số và dấu _")]
        [Display(Name = "Tên đăng nhập")]
        public string Username { get; set; }

       
        //[StringLength(255, MinimumLength = 8, ErrorMessage = "Mật khẩu phải từ 8-255 ký tự")]
        //[DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string PasswordHash { get; set; }
    } 
}