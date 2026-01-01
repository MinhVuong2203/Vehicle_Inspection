using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("Position")]
public partial class Position
{
    [Key]
    public int PositionId { get; set; }

    [StringLength(100)]
    public string PoitionCode { get; set; } = null!;

    [StringLength(100)]
    public string PositionName { get; set; } = null!;

    [InverseProperty("Position")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
