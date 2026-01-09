using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("Certificate")]
[Index("InspectionId", Name = "UQ_Certificate_Inspection", IsUnique = true)]
[Index("StickerNo", Name = "UQ__Certific__49D9EC73D64638A9", IsUnique = true)]
[Index("CertificateNo", Name = "UQ__Certific__BBF8ECEB422987A9", IsUnique = true)]
public partial class Certificate
{
    [Key]
    public int CertificateId { get; set; }

    public int InspectionId { get; set; }

    [StringLength(40)]
    public string CertificateNo { get; set; } = null!;

    [StringLength(40)]
    public string? StickerNo { get; set; }

    public DateOnly IssueDate { get; set; }

    public DateOnly ExpiryDate { get; set; }

    public int ValidityMonths { get; set; }

    public short Status { get; set; }

    [StringLength(50)]
    public string? PrintTemplate { get; set; }

    public int? PrintCount { get; set; }

    public DateTime? LastPrintedAt { get; set; }

    public Guid? IssuedBy { get; set; }

    public DateTime IssuedAt { get; set; }

    [StringLength(500)]
    public string? PdfUrl { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    [ForeignKey("InspectionId")]
    [InverseProperty("Certificate")]
    public virtual Inspection Inspection { get; set; } = null!;

    [ForeignKey("IssuedBy")]
    [InverseProperty("Certificates")]
    public virtual User? IssuedByNavigation { get; set; }
}
