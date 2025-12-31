using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Vehicle_Inspection.Models;

namespace Vehicle_Inspection.Data;

public partial class VehInsContext : DbContext
{
    public VehInsContext(DbContextOptions<VehInsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Role> Roles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE1A6604636D");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
