using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("InspectionStage")]
[Index("InspectionId", "StageId", Name = "UQ_InspStage", IsUnique = true)]
public partial class InspectionStage
{
    [Key]
    public long InspStageId { get; set; }

    public int InspectionId { get; set; }

    public int StageId { get; set; }

    public int Status { get; set; }

    public int? StageResult { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    public int SortOrder { get; set; }

    public bool IsRequired { get; set; }

    [ForeignKey("InspectionId")]
    [InverseProperty("InspectionStages")]
    public virtual Inspection Inspection { get; set; } = null!;

    [InverseProperty("InspStage")]
    public virtual ICollection<InspectionDefect> InspectionDefects { get; set; } = new List<InspectionDefect>();

    [InverseProperty("InspStage")]
    public virtual ICollection<InspectionDetail> InspectionDetails { get; set; } = new List<InspectionDetail>();

    [ForeignKey("StageId")]
    [InverseProperty("InspectionStages")]
    public virtual Stage Stage { get; set; } = null!;
}
