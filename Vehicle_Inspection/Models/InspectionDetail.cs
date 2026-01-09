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

    [StringLength(100)]
    public string? StandardText { get; set; }

    [Column(TypeName = "decimal(18, 4)")]
    public decimal? ActualValue { get; set; }

    [StringLength(100)]
    public string? ActualText { get; set; }

    [StringLength(20)]
    public string? Unit { get; set; }

    public bool? IsPassed { get; set; }

    [Column(TypeName = "decimal(10, 2)")]
    public decimal? DeviationPercent { get; set; }

    [StringLength(20)]
    public string DataSource { get; set; } = null!;

    [StringLength(50)]
    public string? DeviceId { get; set; }

    public DateTime RecordedAt { get; set; }

    [StringLength(1000)]
    public string? ImageUrls { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [ForeignKey("InspStageId")]
    [InverseProperty("InspectionDetails")]
    public virtual InspectionStage InspStage { get; set; } = null!;

    [ForeignKey("ItemId")]
    [InverseProperty("InspectionDetails")]
    public virtual StageItem Item { get; set; } = null!;
}
