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

    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<PasswordRecovery> PasswordRecoveries { get; set; }

    public virtual DbSet<Position> Positions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Account__1788CC4C455BB54A");

            entity.Property(e => e.UserId).ValueGeneratedNever();

            entity.HasOne(d => d.User).WithOne(p => p.Account).HasConstraintName("FK_Account_User");
        });

        modelBuilder.Entity<PasswordRecovery>(entity =>
        {
            entity.HasKey(e => e.PasswordRecoveryId).HasName("PK__Password__A7DC5AFD77A1EB30");

            entity.HasOne(d => d.User).WithMany(p => p.PasswordRecoveries).HasConstraintName("FK__PasswordR__UserI__498EEC8D");
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.PositionId).HasName("PK__Position__60BB9A7921591801");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE1A6604636D");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.TeamId).HasName("PK__Team__123AE799C3A57D94");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CC4C78B3D23E");

            entity.Property(e => e.UserId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PositionId).HasDefaultValue(1);
            entity.Property(e => e.TeamId).HasDefaultValue(1);

            entity.HasOne(d => d.Position).WithMany(p => p.Users).HasConstraintName("FK__User__PositionId__3F115E1A");

            entity.HasOne(d => d.Team).WithMany(p => p.Users).HasConstraintName("FK__User__TeamId__40058253");

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "User_Role",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("FK__User_Role__RoleI__4D5F7D71"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK__User_Role__UserI__4C6B5938"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId").HasName("PK__User_Rol__AF2760ADC8FA6948");
                        j.ToTable("User_Role");
                    });
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
