using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("Account")]
[Index("Username", Name = "UQ__Account__536C85E479D7B1E2", IsUnique = true)]
public partial class Account
{
    [Key]
    public Guid UserId { get; set; }

    [StringLength(50)]
    public string Username { get; set; } = null!;

    [StringLength(255)]
    public string PasswordHash { get; set; } = null!;

    public bool IsLocked { get; set; }

    public int FailedCount { get; set; }

    public DateTime? LastLoginAt { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Account")]
    public virtual User User { get; set; } = null!;
}
