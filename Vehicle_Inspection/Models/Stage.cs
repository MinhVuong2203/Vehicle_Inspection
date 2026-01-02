using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("Stage")]
[Index("StageCode", Name = "UQ__Stage__7BFA4BE3D939D9F9", IsUnique = true)]
public partial class Stage
{
    [Key]
    public int StageId { get; set; }

    [StringLength(30)]
    public string StageCode { get; set; } = null!;

    [StringLength(120)]
    public string StageName { get; set; } = null!;

    public int SortOrder { get; set; }

    public bool? IsActive { get; set; }

    [InverseProperty("Stage")]
    public virtual ICollection<InspectionStage> InspectionStages { get; set; } = new List<InspectionStage>();

    [InverseProperty("Stage")]
    public virtual ICollection<LaneStage> LaneStages { get; set; } = new List<LaneStage>();

    [InverseProperty("Stage")]
    public virtual ICollection<StageItem> StageItems { get; set; } = new List<StageItem>();

    [ForeignKey("StageId")]
    [InverseProperty("Stages")]
    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
