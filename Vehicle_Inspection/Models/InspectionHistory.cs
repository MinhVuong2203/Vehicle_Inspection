using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("InspectionHistory")]
public partial class InspectionHistory
{
    [Key]
    public long HistoryId { get; set; }

    public int InspectionId { get; set; }

    public short? FromStatus { get; set; }

    public short ToStatus { get; set; }

    public Guid? ChangedBy { get; set; }

    public DateTime ChangedAt { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [ForeignKey("ChangedBy")]
    [InverseProperty("InspectionHistories")]
    public virtual User? ChangedByNavigation { get; set; }

    [ForeignKey("InspectionId")]
    [InverseProperty("InspectionHistories")]
    public virtual Inspection Inspection { get; set; } = null!;
}
