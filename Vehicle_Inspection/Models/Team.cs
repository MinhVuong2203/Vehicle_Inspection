using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("Team")]
public partial class Team
{
    [Key]
    public int TeamId { get; set; }

    [StringLength(100)]
    public string TeamCode { get; set; } = null!;

    [StringLength(100)]
    public string TeamName { get; set; } = null!;

    [InverseProperty("Team")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
