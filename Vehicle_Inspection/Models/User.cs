using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("User")]
[Index("Phone", Name = "UQ__User__5C7E359E72C1AE14", IsUnique = true)]
[Index("CCCD", Name = "UQ__User__A955A0AAE181CA80", IsUnique = true)]
[Index("Email", Name = "UQ__User__A9D10534A5911383", IsUnique = true)]
public partial class User
{
    [Key]
    public Guid UserId { get; set; }

    [StringLength(120)]
    public string FullName { get; set; } = null!;

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(120)]
    public string? Email { get; set; }

    public DateOnly? BirthDate { get; set; }

    [StringLength(20)]
    public string? CCCD { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    [StringLength(10)]
    public string? Gender { get; set; }

    [StringLength(255)]
    public string? ImageUrl { get; set; }

    public int? PositionId { get; set; }

    public int? TeamId { get; set; }

    [StringLength(50)]
    public string? Level { get; set; }

    public bool IsActive { get; set; }

    [Column(TypeName = "datetime")]
    public DateTime CreatedAt { get; set; }

    [InverseProperty("User")]
    public virtual Account? Account { get; set; }

    [InverseProperty("User")]
    public virtual ICollection<PasswordRecovery> PasswordRecoveries { get; set; } = new List<PasswordRecovery>();

    [ForeignKey("PositionId")]
    [InverseProperty("Users")]
    public virtual Position? Position { get; set; }

    [ForeignKey("TeamId")]
    [InverseProperty("Users")]
    public virtual Team? Team { get; set; }

    [ForeignKey("UserId")]
    [InverseProperty("Users")]
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
