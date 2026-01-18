using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vehicle_Inspection.Models;

[Table("Account")]
public partial class Account
{
    [Key]
    public Guid UserId { get; set; }

    [StringLength(50)]
    public string? Username { get; set; }

    [StringLength(255)]
    public string? PasswordHash { get; set; }

    public bool IsLocked { get; set; }

    public int FailedCount { get; set; }

    public DateTime? LastLoginAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Account")]
    [ValidateNever]
    public virtual User User { get; set; } = null!;
}
