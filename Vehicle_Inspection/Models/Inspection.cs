using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("Inspection")]
[Index("InspectionCode", Name = "UQ__Inspecti__2DDF04D245C48AA4", IsUnique = true)]
public partial class Inspection
{
    [Key]
    public int InspectionId { get; set; }

    [StringLength(30)]
    public string InspectionCode { get; set; } = null!;

    public int VehicleId { get; set; }

    public Guid OwnerId { get; set; }

    [StringLength(20)]
    public string InspectionType { get; set; } = null!;

    public int? ParentInspectionId { get; set; }

    public int? LaneId { get; set; }

    public short Status { get; set; }

    public int? FinalResult { get; set; }

    [StringLength(1000)]
    public string? ConclusionNote { get; set; }

    public Guid? ConcludedBy { get; set; }

    public DateTime? ConcludedAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ReceivedAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime? StartedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime? CertifiedAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? ReceivedBy { get; set; }

    [StringLength(1000)]
    public string? Notes { get; set; }

    public short? Priority { get; set; }

    public bool IsDeleted { get; set; }

    [InverseProperty("Inspection")]
    public virtual Certificate? Certificate { get; set; }

    [ForeignKey("ConcludedBy")]
    [InverseProperty("InspectionConcludedByNavigations")]
    public virtual User? ConcludedByNavigation { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("InspectionCreatedByNavigations")]
    public virtual User? CreatedByNavigation { get; set; }

    [InverseProperty("Inspection")]
    public virtual ICollection<InspectionDefect> InspectionDefects { get; set; } = new List<InspectionDefect>();

    [InverseProperty("Inspection")]
    public virtual ICollection<InspectionHistory> InspectionHistories { get; set; } = new List<InspectionHistory>();

    [InverseProperty("Inspection")]
    public virtual ICollection<InspectionStage> InspectionStages { get; set; } = new List<InspectionStage>();

    [InverseProperty("ParentInspection")]
    public virtual ICollection<Inspection> InverseParentInspection { get; set; } = new List<Inspection>();

    [ForeignKey("LaneId")]
    [InverseProperty("Inspections")]
    public virtual Lane? Lane { get; set; }

    [ForeignKey("OwnerId")]
    [InverseProperty("Inspections")]
    public virtual Owner Owner { get; set; } = null!;

    [ForeignKey("ParentInspectionId")]
    [InverseProperty("InverseParentInspection")]
    public virtual Inspection? ParentInspection { get; set; }

    [InverseProperty("Inspection")]
    public virtual Payment? Payment { get; set; }

    [ForeignKey("ReceivedBy")]
    [InverseProperty("InspectionReceivedByNavigations")]
    public virtual User? ReceivedByNavigation { get; set; }

    [ForeignKey("VehicleId")]
    [InverseProperty("Inspections")]
    public virtual Vehicle Vehicle { get; set; } = null!;
}
