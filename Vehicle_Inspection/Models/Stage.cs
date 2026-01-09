using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("Stage")]
[Index("StageCode", Name = "UQ__Stage__7BFA4BE322245640", IsUnique = true)]
public partial class Stage
{
    [Key]
    public int StageId { get; set; }

    [StringLength(30)]
    public string StageCode { get; set; } = null!;

    [StringLength(120)]
    public string StageName { get; set; } = null!;

    public bool? IsActive { get; set; }

    [InverseProperty("Stage")]
    public virtual ICollection<InspectionStage> InspectionStages { get; set; } = new List<InspectionStage>();

    [InverseProperty("Stage")]
    public virtual ICollection<LaneStage> LaneStages { get; set; } = new List<LaneStage>();

    [InverseProperty("Stage")]
    public virtual ICollection<StageItem> StageItems { get; set; } = new List<StageItem>();
}
