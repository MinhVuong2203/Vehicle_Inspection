using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("Owner")]
public partial class Owner
{
    [Key]
    public Guid OwnerId { get; set; }

    [StringLength(20)]
    public string OwnerType { get; set; } = null!;

    [StringLength(150)]
    public string FullName { get; set; } = null!;

    [StringLength(30)]
    public string? CCCD { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    [StringLength(120)]
    public string? Email { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    public DateTime CreatedAt { get; set; }

    [InverseProperty("Owner")]
    public virtual ICollection<Inspection> Inspections { get; set; } = new List<Inspection>();

    [InverseProperty("Owner")]
    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
