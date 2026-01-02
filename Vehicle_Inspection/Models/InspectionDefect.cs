using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("InspectionDefect")]
public partial class InspectionDefect
{
    [Key]
    public long DefectId { get; set; }

    public int InspectionId { get; set; }

    public long? InspStageId { get; set; }

    public int? ItemId { get; set; }

    [StringLength(50)]
    public string DefectCategory { get; set; } = null!;

    [StringLength(40)]
    public string? DefectCode { get; set; }

    [StringLength(1000)]
    public string DefectDescription { get; set; } = null!;

    public int Severity { get; set; }

    [StringLength(1000)]
    public string? ImageUrls { get; set; }

    public bool IsFixed { get; set; }

    [StringLength(500)]
    public string? FixedNote { get; set; }

    public Guid? VerifiedBy { get; set; }

    public DateTime? VerifiedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("InspectionDefectCreatedByNavigations")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("InspStageId")]
    [InverseProperty("InspectionDefects")]
    public virtual InspectionStage? InspStage { get; set; }

    [ForeignKey("InspectionId")]
    [InverseProperty("InspectionDefects")]
    public virtual Inspection Inspection { get; set; } = null!;

    [ForeignKey("ItemId")]
    [InverseProperty("InspectionDefects")]
    public virtual StageItem? Item { get; set; }

    [ForeignKey("VerifiedBy")]
    [InverseProperty("InspectionDefectVerifiedByNavigations")]
    public virtual User? VerifiedByNavigation { get; set; }
}
