using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("StageItemThreshold")]
[Index("ItemId", "VehicleTypeId", "EffectiveDate", Name = "UQ_ItemVehicleDate", IsUnique = true)]
public partial class StageItemThreshold
{
    [Key]
    public int ThresholdId { get; set; }

    public int ItemId { get; set; }

    public int VehicleTypeId { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal? MinValue { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal? MaxValue { get; set; }

    [StringLength(200)]
    public string? PassCondition { get; set; }

    [StringLength(500)]
    public string? AllowedValues { get; set; }

    [StringLength(20)]
    public string? FailAction { get; set; }

    public bool? IsActive { get; set; }

    public DateOnly? EffectiveDate { get; set; }

    [ForeignKey("ItemId")]
    [InverseProperty("StageItemThresholds")]
    public virtual StageItem Item { get; set; } = null!;

    [ForeignKey("VehicleTypeId")]
    [InverseProperty("StageItemThresholds")]
    public virtual VehicleType VehicleType { get; set; } = null!;
}
