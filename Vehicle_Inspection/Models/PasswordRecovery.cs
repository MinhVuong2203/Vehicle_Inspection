using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("PasswordRecovery")]
public partial class PasswordRecovery
{
    [Key]
    public int PasswordRecoveryId { get; set; }

    public Guid UserId { get; set; }

    [StringLength(200)]
    public string? ResetOtpHash { get; set; }

    public DateTime? ResetOtpExpiresAt { get; set; }

    public int ResetOtpAttemptCount { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("PasswordRecoveries")]
    public virtual User User { get; set; } = null!;
}
