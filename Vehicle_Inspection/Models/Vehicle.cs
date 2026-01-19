using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("Vehicle")]
[Index("PlateNo", Name = "UQ_Vehicle_PlateNo", IsUnique = true)]
public partial class Vehicle
{
    [Key]
    public int VehicleId { get; set; }

    [StringLength(20)]
    public string PlateNo { get; set; } = null!;

    [StringLength(50)]
    public string? InspectionNo { get; set; }

    [StringLength(100)]
    public string? VehicleGroup { get; set; }

    [StringLength(50)]
    public string? EnergyType { get; set; }

    public bool? IsCleanEnergy { get; set; }

    [StringLength(20)]
    public string? UsagePermission { get; set; }

    [StringLength(100)]
    public string? Brand { get; set; }

    [StringLength(100)]
    public string? Model { get; set; }

    [StringLength(50)]
    public string? EngineNo { get; set; }

    [StringLength(50)]
    public string? Chassis { get; set; }

    public int? ManufactureYear { get; set; }

    [StringLength(100)]
    public string? ManufactureCountry { get; set; }

    public int? LifetimeLimitYear { get; set; }

    public bool? HasCommercialModification { get; set; }

    public bool? HasModification { get; set; }

    public Guid OwnerId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? UpdatedBy { get; set; }

    public int? VehicleTypeId { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("VehicleCreatedByNavigations")]
    public virtual User? CreatedByNavigation { get; set; }

    [InverseProperty("Vehicle")]
    public virtual ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();

    [ForeignKey("OwnerId")]
    [InverseProperty("Vehicles")]
    public virtual Owner Owner { get; set; } = null!;

    [InverseProperty("PlateNoNavigation")]
    public virtual Specification? Specification { get; set; }

    [ForeignKey("UpdatedBy")]
    [InverseProperty("VehicleUpdatedByNavigations")]
    public virtual User? UpdatedByNavigation { get; set; }

    [ForeignKey("VehicleTypeId")]
    [InverseProperty("Vehicles")]
    public virtual VehicleType? VehicleType { get; set; }
}
