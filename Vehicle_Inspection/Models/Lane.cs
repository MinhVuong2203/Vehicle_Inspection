using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("Lane")]
[Index("LaneCode", Name = "UQ__Lane__8A8AD4B986F5A427", IsUnique = true)]
public partial class Lane
{
    [Key]
    public int LaneId { get; set; }

    [StringLength(20)]
    public string LaneCode { get; set; } = null!;

    [StringLength(100)]
    public string LaneName { get; set; } = null!;

    public bool IsActive { get; set; }

    [InverseProperty("Lane")]
    public virtual ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();

    [InverseProperty("Lane")]
    public virtual ICollection<LaneStage> LaneStages { get; set; } = new List<LaneStage>();

    [ForeignKey("LaneId")]
    [InverseProperty("Lanes")]
    public virtual ICollection<VehicleType> VehicleTypes { get; set; } = new List<VehicleType>();
}
