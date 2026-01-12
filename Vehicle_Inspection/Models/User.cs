using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("User")]
[Index("CCCD", Name = "UQ_User_CCCD", IsUnique = true)]
[Index("Email", Name = "UQ_User_Email", IsUnique = true)]
[Index("Phone", Name = "UQ_User_Phone", IsUnique = true)]
public partial class User
{
    [Key]
    public Guid UserId { get; set; }

    [StringLength(120)]
    public string FullName { get; set; } = null!;

    [StringLength(20)]
    public string Phone { get; set; } = null!;

    [StringLength(120)]
    public string Email { get; set; } = null!;

    public DateOnly? BirthDate { get; set; }

    [StringLength(20)]
    public string CCCD { get; set; } = null!;

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

    [StringLength(100)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? Ward { get; set; }

    [StringLength(100)]
    public string? Province { get; set; }

    [InverseProperty("User")]
    public virtual Account? Account { get; set; }

    [InverseProperty("IssuedByNavigation")]
    public virtual ICollection<Certificate> Certificates { get; set; } = new List<Certificate>();

    [InverseProperty("ConcludedByNavigation")]
    public virtual ICollection<Inspection> InspectionConcludedByNavigations { get; set; } = new List<Inspection>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Inspection> InspectionCreatedByNavigations { get; set; } = new List<Inspection>();

    [InverseProperty("VerifiedByNavigation")]
    public virtual ICollection<InspectionDefect> InspectionDefects { get; set; } = new List<InspectionDefect>();

    [InverseProperty("ReceivedByNavigation")]
    public virtual ICollection<Inspection> InspectionReceivedByNavigations { get; set; } = new List<Inspection>();

    [InverseProperty("AssignedUser")]
    public virtual ICollection<InspectionStage> InspectionStages { get; set; } = new List<InspectionStage>();

    [InverseProperty("User")]
    public virtual ICollection<PasswordRecovery> PasswordRecoveries { get; set; } = new List<PasswordRecovery>();

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Payment> PaymentCreatedByNavigations { get; set; } = new List<Payment>();

    [InverseProperty("PaidByNavigation")]
    public virtual ICollection<Payment> PaymentPaidByNavigations { get; set; } = new List<Payment>();

    [ForeignKey("PositionId")]
    [InverseProperty("Users")]
    public virtual Position? Position { get; set; }

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Specification> SpecificationCreatedByNavigations { get; set; } = new List<Specification>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<Specification> SpecificationUpdatedByNavigations { get; set; } = new List<Specification>();

    [ForeignKey("TeamId")]
    [InverseProperty("Users")]
    public virtual Team? Team { get; set; }

    [InverseProperty("CreatedByNavigation")]
    public virtual ICollection<Vehicle> VehicleCreatedByNavigations { get; set; } = new List<Vehicle>();

    [InverseProperty("UpdatedByNavigation")]
    public virtual ICollection<Vehicle> VehicleUpdatedByNavigations { get; set; } = new List<Vehicle>();

    [ForeignKey("UserId")]
    [InverseProperty("Users")]
    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();

    [ForeignKey("UserId")]
    [InverseProperty("Users")]
    public virtual ICollection<Stage> Stages { get; set; } = new List<Stage>();
}
