using System;
using System.ComponentModel.DataAnnotations;
using Vehicle_Inspection.Models;

namespace Vehicle_Inspection.Models.Validation
{
    [AttributeUsage(AttributeTargets.Class)]
    public class SpecificationValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object? obj)
        {
            if (obj is not Specification spec)
                return true;

            // --- Validate: Kích thước lòng thùng phải nhỏ hơn kích thước bao ---
            if (spec.CargoInsideLength.HasValue && spec.OverallLength.HasValue)
            {
                if (spec.CargoInsideLength.Value >= spec.OverallLength.Value)
                {
                    ErrorMessage = "Chiều dài lòng thùng phải nhỏ hơn chiều dài bao.";
                    return false;
                }
            }
            if (spec.CargoInsideWidth.HasValue && spec.OverallWidth.HasValue)
            {
                if (spec.CargoInsideWidth.Value >= spec.OverallWidth.Value)
                {
                    ErrorMessage = "Chiều rộng lòng thùng phải nhỏ hơn chiều rộng bao.";
                    return false;
                }
            }
            if (spec.CargoInsideHeight.HasValue && spec.OverallHeight.HasValue)
            {
                if (spec.CargoInsideHeight.Value >= spec.OverallHeight.Value)
                {
                    ErrorMessage = "Chiều cao lòng thùng phải nhỏ hơn chiều cao bao.";
                    return false;
                }
            }

            // --- Validate: Khối lượng toàn bộ >= Khối lượng bản thân ---
            if (spec.AuthorizedTotalWeight.HasValue && spec.KerbWeight.HasValue)
            {
                if (spec.AuthorizedTotalWeight.Value < spec.KerbWeight.Value)
                {
                    ErrorMessage = "Khối lượng toàn bộ phải lớn hơn hoặc bằng khối lượng bản thân.";
                    return false;
                }
            }

            // --- Validate: Khối lượng toàn bộ >= Khối lượng bản thân + Khối lượng hàng CC ---
            if (spec.AuthorizedTotalWeight.HasValue && spec.KerbWeight.HasValue && spec.AuthorizedCargoWeight.HasValue)
            {
                if (spec.AuthorizedTotalWeight.Value < spec.KerbWeight.Value + spec.AuthorizedCargoWeight.Value)
                {
                    ErrorMessage = "Khối lượng toàn bộ phải lớn hơn hoặc bằng tổng của khối lượng bản thân và khối lượng hàng CC.";
                    return false;
                }
            }

            // --- Validate: Nếu có động cơ điện thì phải có số lượng động cơ > 0 ---
            if (!string.IsNullOrWhiteSpace(spec.MotorType) && (!spec.NumberOfMotors.HasValue || spec.NumberOfMotors.Value < 1))
            {
                ErrorMessage = "Khi có loại động cơ điện, số lượng động cơ điện phải ít nhất là 1.";
                return false;
            }

            // --- Validate: Nếu có số lượng động cơ điện > 0 thì phải có loại động cơ điện ---
            if (spec.NumberOfMotors.HasValue && spec.NumberOfMotors.Value > 0 && string.IsNullOrWhiteSpace(spec.MotorType))
            {
                ErrorMessage = "Khi số lượng động cơ điện > 0, loại động cơ điện không được để trống.";
                return false;
            }

            // --- Validate: Nếu có loại ắc quy thì phải có điện áp và dung lượng ---
            if (!string.IsNullOrWhiteSpace(spec.BatteryType))
            {
                if (!spec.BatteryVoltage.HasValue || !spec.BatteryCapacity.HasValue)
                {
                    ErrorMessage = "Khi có loại ắc quy, điện áp và dung lượng ắc quy không được để trống.";
                    return false;
                }
            }

            // --- Validate: Nếu có cỡ lốp thì phải có số lượng lốp > 0 ---
            if (!string.IsNullOrWhiteSpace(spec.TireSize) && (!spec.TireCount.HasValue || spec.TireCount.Value < 1))
            {
                ErrorMessage = "Khi có cỡ lốp, số lượng lốp phải ít nhất là 1.";
                return false;
            }

            // --- Validate: Nếu có số lượng lốp > 0 thì phải có cỡ lốp ---
            if (spec.TireCount.HasValue && spec.TireCount.Value > 0 && string.IsNullOrWhiteSpace(spec.TireSize))
            {
                ErrorMessage = "Khi số lượng lốp > 0, cỡ lốp không được để trống.";
                return false;
            }

            // --- Validate: Wheelbase phải nhỏ hơn OverallLength ---
            if (spec.Wheelbase.HasValue && spec.OverallLength.HasValue)
            {
                if (spec.Wheelbase.Value >= spec.OverallLength.Value)
                {
                    ErrorMessage = "Khoảng cách trục phải nhỏ hơn chiều dài bao.";
                    return false;
                }
            }

            return true;
        }
    }
}