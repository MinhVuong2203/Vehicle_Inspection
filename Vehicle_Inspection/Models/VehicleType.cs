using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("VehicleType")]
[Index("TypeCode", Name = "UQ__VehicleT__3E1CDC7CB3FCB60F", IsUnique = true)]
public partial class VehicleType
{
    [Key]
    public int VehicleTypeId { get; set; }

    [StringLength(20)]
    public string TypeCode { get; set; } = null!;

    [StringLength(100)]
    public string TypeName { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    public bool? IsActive { get; set; }

    [InverseProperty("VehicleType")]
    public virtual ICollection<FeeSchedule> FeeSchedules { get; set; } = new List<FeeSchedule>();

    [InverseProperty("VehicleType")]
    public virtual ICollection<StageItemThreshold> StageItemThresholds { get; set; } = new List<StageItemThreshold>();
}
