using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("InspectionDetail")]
[Index("InspStageId", "ItemId", Name = "UQ_InspDetail", IsUnique = true)]
public partial class InspectionDetail
{
    [Key]
    public int DetailId { get; set; }

    public long InspStageId { get; set; }

    public int ItemId { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal? StandardMin { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal? StandardMax { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal? ActualValue { get; set; }

    [StringLength(20)]
    public string? Unit { get; set; }

    public bool? IsPassed { get; set; }

    [StringLength(20)]
    public string DataSource { get; set; } = null!;

    public DateTime RecordedAt { get; set; }

    [ForeignKey("InspStageId")]
    [InverseProperty("InspectionDetails")]
    public virtual InspectionStage InspStage { get; set; } = null!;

    [ForeignKey("ItemId")]
    [InverseProperty("InspectionDetails")]
    public virtual StageItem Item { get; set; } = null!;
}
