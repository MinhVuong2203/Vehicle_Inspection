using System.ComponentModel.DataAnnotations;


namespace Vehicle_Inspection.Models.Validation
{
    public class VehicleValidation : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            var vehicle = validationContext.ObjectInstance as Vehicle;
            if (vehicle == null) return ValidationResult.Success;

            // Kiểm tra năm sản xuất không được lớn hơn năm hiện tại
            if (vehicle.ManufactureYear.HasValue && vehicle.ManufactureYear.Value > DateTime.Now.Year)
            {
                return new ValidationResult(
                    $"Năm sản xuất không được lớn hơn năm hiện tại ({DateTime.Now.Year})",
                    new[] { "ManufactureYear" }
                );
            }

            // Kiểm tra nếu có năng lượng sạch thì phải có loại năng lượng
            if (vehicle.IsCleanEnergy == true && string.IsNullOrWhiteSpace(vehicle.EnergyType))
            {
                return new ValidationResult(
                    "Vui lòng chọn loại năng lượng khi xe sử dụng năng lượng sạch",
                    new[] { "EnergyType" }
                );
            }

            // Kiểm tra nếu có cải tạo kinh doanh thì phải đánh dấu đã cải tạo
            if (vehicle.HasCommercialModification == true && vehicle.HasModification != true)
            {
                return new ValidationResult(
                    "Xe kinh doanh vận tải phải được đánh dấu là đã cải tạo",
                    new[] { "HasModification" }
                );
            }

            // Kiểm tra niên hạn sử dụng hợp lý với năm sản xuất
            if (vehicle.ManufactureYear.HasValue && vehicle.LifetimeLimitYear.HasValue)
            {
                int expiryYear = vehicle.ManufactureYear.Value + vehicle.LifetimeLimitYear.Value;
                if (expiryYear < DateTime.Now.Year)
                {
                    return new ValidationResult(
                        $"Xe đã hết niên hạn sử dụng (hết hạn năm {expiryYear})",
                        new[] { "LifetimeLimitYear" }
                    );
                }
            }

            return ValidationResult.Success;
        }
    }
}
