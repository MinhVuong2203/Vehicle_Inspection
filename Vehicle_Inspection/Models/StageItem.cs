using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("StageItem")]
[Index("StageId", "ItemCode", Name = "UQ_StageItem", IsUnique = true)]
public partial class StageItem
{
    [Key]
    public int ItemId { get; set; }

    public int StageId { get; set; }

    [StringLength(40)]
    public string ItemCode { get; set; } = null!;

    [StringLength(160)]
    public string ItemName { get; set; } = null!;

    [StringLength(20)]
    public string? Unit { get; set; }

    [StringLength(20)]
    public string DataType { get; set; } = null!;

    public int? SortOrder { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    public bool IsRequired { get; set; }

    [InverseProperty("Item")]
    public virtual ICollection<InspectionDefect> InspectionDefects { get; set; } = new List<InspectionDefect>();

    [InverseProperty("Item")]
    public virtual ICollection<InspectionDetail> InspectionDetails { get; set; } = new List<InspectionDetail>();

    [ForeignKey("StageId")]
    [InverseProperty("StageItems")]
    public virtual Stage Stage { get; set; } = null!;

    [InverseProperty("Item")]
    public virtual ICollection<StageItemThreshold> StageItemThresholds { get; set; } = new List<StageItemThreshold>();
}
