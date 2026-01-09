using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("Payment")]
[Index("InspectionId", Name = "UQ_Payment_Inspection", IsUnique = true)]
[Index("ReceiptNo", Name = "UQ__Payment__CC0B72A65D3A5FE8", IsUnique = true)]
public partial class Payment
{
    [Key]
    public int PaymentId { get; set; }

    public int InspectionId { get; set; }

    public int? FeeScheduleId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal BaseFee { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? CertificateFee { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? StickerFee { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalAmount { get; set; }

    [StringLength(30)]
    public string PaymentMethod { get; set; } = null!;

    public short PaymentStatus { get; set; }

    [StringLength(40)]
    public string? ReceiptNo { get; set; }

    public int? ReceiptPrintCount { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public Guid? CreatedBy { get; set; }

    public Guid? PaidBy { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [ForeignKey("CreatedBy")]
    [InverseProperty("PaymentCreatedByNavigations")]
    public virtual User? CreatedByNavigation { get; set; }

    [ForeignKey("FeeScheduleId")]
    [InverseProperty("Payments")]
    public virtual FeeSchedule? FeeSchedule { get; set; }

    [ForeignKey("InspectionId")]
    [InverseProperty("Payment")]
    public virtual Inspection Inspection { get; set; } = null!;

    [ForeignKey("PaidBy")]
    [InverseProperty("PaymentPaidByNavigations")]
    public virtual User? PaidByNavigation { get; set; }
}
