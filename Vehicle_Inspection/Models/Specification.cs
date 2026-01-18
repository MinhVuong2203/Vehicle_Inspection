using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("Specification")]
[Index("PlateNo", Name = "UQ__Specific__48227C0CB9D8BDAA", IsUnique = true)]
public partial class Specification
{
    [Key]
    public int SpecificationId { get; set; }

    [StringLength(20)]
    public string PlateNo { get; set; } = null!;

    [StringLength(50)]
    public string? WheelFormula { get; set; }

    public int? WheelTread { get; set; }

    public int? OverallLength { get; set; }

    public int? OverallWidth { get; set; }

    public int? OverallHeight { get; set; }

    public int? CargoInsideLength { get; set; }

    public int? CargoInsideWidth { get; set; }

    public int? CargoInsideHeight { get; set; }

    public int? Wheelbase { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? KerbWeight { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? AuthorizedCargoWeight { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? AuthorizedTowedWeight { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? AuthorizedTotalWeight { get; set; }

    public int? SeatingCapacity { get; set; }

    public int? StandingCapacity { get; set; }

    public int? LyingCapacity { get; set; }

    [StringLength(100)]
    public string? EngineType { get; set; }

    [StringLength(50)]
    public string? EnginePosition { get; set; }

    [StringLength(50)]
    public string? EngineModel { get; set; }

    public int? EngineDisplacement { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? MaxPower { get; set; }

    public int? MaxPowerRPM { get; set; }

    [StringLength(50)]
    public string? FuelType { get; set; }

    [StringLength(100)]
    public string? MotorType { get; set; }

    public int? NumberOfMotors { get; set; }

    [StringLength(50)]
    public string? MotorModel { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? TotalMotorPower { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? MotorVoltage { get; set; }

    [StringLength(100)]
    public string? BatteryType { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? BatteryVoltage { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? BatteryCapacity { get; set; }

    public int? TireCount { get; set; }

    [StringLength(50)]
    public string? TireSize { get; set; }

    [StringLength(100)]
    public string? TireAxleInfo { get; set; }

    [StringLength(100)]
    public string? ImagePosition { get; set; }

    public bool? HasTachograph { get; set; }

    public bool? HasDriverCamera { get; set; }

    public bool? NotIssuedStamp { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("SpecificationCreatedByNavigations")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("PlateNo")]
    [InverseProperty("Specification")]
    public virtual Vehicle PlateNoNavigation { get; set; } = null!;

    [ForeignKey("UpdatedBy")]
    [InverseProperty("SpecificationUpdatedByNavigations")]
    public virtual User? UpdatedByNavigation { get; set; }
}
