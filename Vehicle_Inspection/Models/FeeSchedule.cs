using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("FeeSchedule")]
public partial class FeeSchedule
{
    [Key]
    public int FeeId { get; set; }

    [StringLength(30)]
    public string ServiceType { get; set; } = null!;

    public int? VehicleTypeId { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal BaseFee { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? CertificateFee { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal? StickerFee { get; set; }

    [Column(TypeName = "decimal(18, 2)")]
    public decimal TotalFee { get; set; }

    public DateOnly EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public bool IsActive { get; set; }

    public Guid? CreatedBy { get; set; }

    public DateTime CreatedAt { get; set; }

    public Guid? UpdatedBy { get; set; }

    public DateTime? UpdatedAt { get; set; }

    [InverseProperty("FeeSchedule")]
    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    [ForeignKey("VehicleTypeId")]
    [InverseProperty("FeeSchedules")]
    public virtual VehicleType? VehicleType { get; set; }
}
