using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("Owner")]
[Index("Phone", Name = "UQ__Owner__5C7E359EF5AF1145", IsUnique = true)]
public partial class Owner
{
    [Key]
    public Guid OwnerId { get; set; }

    [StringLength(20)]
    public string OwnerType { get; set; } = null!;

    [StringLength(150)]
    public string FullName { get; set; } = null!;

    [StringLength(200)]
    public string? CompanyName { get; set; }

    [StringLength(30)]
    public string? TaxCode { get; set; }

    [StringLength(30)]
    public string? CCCD { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(120)]
    public string? Email { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; }

    [StringLength(100)]
    public string? Ward { get; set; }

    [StringLength(100)]
    public string? Province { get; set; }

    [StringLength(255)]
    public string? ImageUrl { get; set; }

    [InverseProperty("Owner")]
    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
