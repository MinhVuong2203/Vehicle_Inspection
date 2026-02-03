using System.ComponentModel.DataAnnotations;

namespace Vehicle_Inspection.Models.Validation
{
    /// <summary>
    /// Custom validation để kiểm tra logic phụ thuộc giữa các trường
    /// </summary>
    public class OwnerValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var owner = (Owner)validationContext.ObjectInstance;

            // Validate theo OwnerType
            if (owner.OwnerType == "PERSON")
            {
                // Cá nhân phải có CCCD
                if (string.IsNullOrWhiteSpace(owner.CCCD))
                {
                    return new ValidationResult(
                        "CCCD/CMND là bắt buộc đối với chủ xe cá nhân",
                        new[] { nameof(Owner.CCCD) }
                    );
                }

                // Cá nhân không nên có CompanyName và TaxCode
                if (!string.IsNullOrWhiteSpace(owner.CompanyName))
                {
                    return new ValidationResult(
                        "Chủ xe cá nhân không được có tên công ty",
                        new[] { nameof(Owner.CompanyName) }
                    );
                }
            }
            else if (owner.OwnerType == "COMPANY")
            {
                // Công ty phải có CompanyName
                if (string.IsNullOrWhiteSpace(owner.CompanyName))
                {
                    return new ValidationResult(
                        "Tên công ty là bắt buộc đối với chủ xe doanh nghiệp",
                        new[] { nameof(Owner.CompanyName) }
                    );
                }

                // Công ty phải có TaxCode
                if (string.IsNullOrWhiteSpace(owner.TaxCode))
                {
                    return new ValidationResult(
                        "Mã số thuế là bắt buộc đối với chủ xe doanh nghiệp",
                        new[] { nameof(Owner.TaxCode) }
                    );
                }
            }

            // Phải có ít nhất một trong hai: Phone hoặc Email
            if (string.IsNullOrWhiteSpace(owner.Phone) && string.IsNullOrWhiteSpace(owner.Email))
            {
                return new ValidationResult(
                    "Phải có ít nhất một thông tin liên lạc (Số điện thoại hoặc Email)"
                );
            }

            return ValidationResult.Success;
        }
    }
}