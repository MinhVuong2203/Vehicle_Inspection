using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("Role")]
[Index("RoleCode", Name = "UQ__Role__D62CB59CB48F1A17", IsUnique = true)]
public partial class Role
{
    [Key]
    public int RoleId { get; set; }

    [StringLength(50)]
    public string RoleCode { get; set; } = null!;

    [StringLength(50)]
    public string RoleAcronym { get; set; } = null!;

    [StringLength(255)]
    public string RoleName { get; set; } = null!;

    [StringLength(255)]
    public string? RoleIcon { get; set; }

    [StringLength(255)]
    public string? RoleHref { get; set; }



}
