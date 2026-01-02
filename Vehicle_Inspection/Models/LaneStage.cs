using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Vehicle_Inspection.Models;

[Table("LaneStage")]
[Index("LaneId", "StageId", Name = "UQ_LaneStage", IsUnique = true)]
public partial class LaneStage
{
    [Key]
    public int LaneStageId { get; set; }

    public int LaneId { get; set; }

    public int StageId { get; set; }

    public int SortOrder { get; set; }

    public bool? IsRequired { get; set; }

    public bool? IsActive { get; set; }

    [ForeignKey("LaneId")]
    [InverseProperty("LaneStages")]
    public virtual Lane Lane { get; set; } = null!;

    [ForeignKey("StageId")]
    [InverseProperty("LaneStages")]
    public virtual Stage Stage { get; set; } = null!;
}
