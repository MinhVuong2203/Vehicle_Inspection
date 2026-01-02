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

    public virtual DbSet<Certificate> Certificates { get; set; }

    public virtual DbSet<FeeSchedule> FeeSchedules { get; set; }

    public virtual DbSet<Inspection> Inspections { get; set; }

    public virtual DbSet<InspectionDefect> InspectionDefects { get; set; }

    public virtual DbSet<InspectionDetail> InspectionDetails { get; set; }

    public virtual DbSet<InspectionHistory> InspectionHistories { get; set; }

    public virtual DbSet<InspectionStage> InspectionStages { get; set; }

    public virtual DbSet<Lane> Lanes { get; set; }

    public virtual DbSet<LaneStage> LaneStages { get; set; }

    public virtual DbSet<Owner> Owners { get; set; }

    public virtual DbSet<PasswordRecovery> PasswordRecoveries { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<Position> Positions { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Specification> Specifications { get; set; }

    public virtual DbSet<Stage> Stages { get; set; }

    public virtual DbSet<StageItem> StageItems { get; set; }

    public virtual DbSet<StageItemThreshold> StageItemThresholds { get; set; }

    public virtual DbSet<Team> Teams { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    public virtual DbSet<VehicleType> VehicleTypes { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__Account__1788CC4C455BB54A");

            entity.Property(e => e.UserId).ValueGeneratedNever();

            entity.HasOne(d => d.User).WithOne(p => p.Account).HasConstraintName("FK_Account_User");
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.CertificateId).HasName("PK__Certific__BBF8A7C1C473CC11");

            entity.Property(e => e.IssuedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.PrintCount).HasDefaultValue(0);
            entity.Property(e => e.PrintTemplate).HasDefaultValue("STANDARD");
            entity.Property(e => e.Status).HasDefaultValue((short)1);
            entity.Property(e => e.ValidityMonths).HasDefaultValue(12);

            entity.HasOne(d => d.Inspection).WithOne(p => p.Certificate)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Certifica__Inspe__45544755");

            entity.HasOne(d => d.IssuedByNavigation).WithMany(p => p.Certificates).HasConstraintName("FK__Certifica__Issue__46486B8E");
        });

        modelBuilder.Entity<FeeSchedule>(entity =>
        {
            entity.HasKey(e => e.FeeId).HasName("PK__FeeSched__B387B2290BEE9886");

            entity.Property(e => e.CertificateFee).HasDefaultValue(0m);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.StickerFee).HasDefaultValue(0m);

            entity.HasOne(d => d.VehicleType).WithMany(p => p.FeeSchedules).HasConstraintName("FK__FeeSchedu__Vehic__2AA05119");
        });

        modelBuilder.Entity<Inspection>(entity =>
        {
            entity.HasKey(e => e.InspectionId).HasName("PK__Inspecti__30B2DC084386CAFE");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.InspectionType).HasDefaultValue("FIRST");
            entity.Property(e => e.Priority).HasDefaultValue((short)1);

            entity.HasOne(d => d.ConcludedByNavigation).WithMany(p => p.InspectionConcludedByNavigations).HasConstraintName("FK__Inspectio__Concl__7814D14C");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InspectionCreatedByNavigations).HasConstraintName("FK__Inspectio__Creat__7908F585");

            entity.HasOne(d => d.Lane).WithMany(p => p.Inspections).HasConstraintName("FK__Inspectio__LaneI__7720AD13");

            entity.HasOne(d => d.Owner).WithMany(p => p.Inspections)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inspectio__Owner__753864A1");

            entity.HasOne(d => d.ParentInspection).WithMany(p => p.InverseParentInspection).HasConstraintName("FK__Inspectio__Paren__762C88DA");

            entity.HasOne(d => d.ReceivedByNavigation).WithMany(p => p.InspectionReceivedByNavigations).HasConstraintName("FK__Inspectio__Recei__79FD19BE");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.Inspections)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inspectio__Vehic__74444068");
        });

        modelBuilder.Entity<InspectionDefect>(entity =>
        {
            entity.HasKey(e => e.DefectId).HasName("PK__Inspecti__144A379CF28C12F3");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.Severity).HasDefaultValue(2);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InspectionDefectCreatedByNavigations).HasConstraintName("FK__Inspectio__Creat__1D4655FB");

            entity.HasOne(d => d.InspStage).WithMany(p => p.InspectionDefects).HasConstraintName("FK__Inspectio__InspS__1B5E0D89");

            entity.HasOne(d => d.Inspection).WithMany(p => p.InspectionDefects).HasConstraintName("FK__Inspectio__Inspe__1A69E950");

            entity.HasOne(d => d.Item).WithMany(p => p.InspectionDefects).HasConstraintName("FK__Inspectio__ItemI__1C5231C2");

            entity.HasOne(d => d.VerifiedByNavigation).WithMany(p => p.InspectionDefectVerifiedByNavigations).HasConstraintName("FK__Inspectio__Verif__1E3A7A34");
        });

        modelBuilder.Entity<InspectionDetail>(entity =>
        {
            entity.HasKey(e => e.DetailId).HasName("PK__Inspecti__135C316D7A8E8DF6");

            entity.Property(e => e.DataSource).HasDefaultValue("MANUAL");
            entity.Property(e => e.RecordedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.InspStage).WithMany(p => p.InspectionDetails).HasConstraintName("FK__Inspectio__InspS__12C8C788");

            entity.HasOne(d => d.Item).WithMany(p => p.InspectionDetails)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inspectio__ItemI__13BCEBC1");

            entity.HasOne(d => d.RecordedByNavigation).WithMany(p => p.InspectionDetails).HasConstraintName("FK__Inspectio__Recor__14B10FFA");
        });

        modelBuilder.Entity<InspectionHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("PK__Inspecti__4D7B4ABD7E1A2AA5");

            entity.Property(e => e.ChangedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.ChangedByNavigation).WithMany(p => p.InspectionHistories).HasConstraintName("FK__Inspectio__Chang__23F3538A");

            entity.HasOne(d => d.Inspection).WithMany(p => p.InspectionHistories).HasConstraintName("FK__Inspectio__Inspe__22FF2F51");
        });

        modelBuilder.Entity<InspectionStage>(entity =>
        {
            entity.HasKey(e => e.InspStageId).HasName("PK__Inspecti__A32B45EA70EFB4D7");

            entity.Property(e => e.IsRequired).HasDefaultValue(true);

            entity.HasOne(d => d.AssignedUser).WithMany(p => p.InspectionStages).HasConstraintName("FK__Inspectio__Assig__056ECC6A");

            entity.HasOne(d => d.Inspection).WithMany(p => p.InspectionStages).HasConstraintName("FK__Inspectio__Inspe__038683F8");

            entity.HasOne(d => d.Stage).WithMany(p => p.InspectionStages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inspectio__Stage__047AA831");
        });

        modelBuilder.Entity<Lane>(entity =>
        {
            entity.HasKey(e => e.LaneId).HasName("PK__Lane__A5770E0C4CB2F96E");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<LaneStage>(entity =>
        {
            entity.HasKey(e => e.LaneStageId).HasName("PK__LaneStag__4A070A392C3E8ED1");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsRequired).HasDefaultValue(true);

            entity.HasOne(d => d.Lane).WithMany(p => p.LaneStages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LaneStage__LaneI__3FD07829");

            entity.HasOne(d => d.Stage).WithMany(p => p.LaneStages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LaneStage__Stage__40C49C62");
        });

        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasKey(e => e.OwnerId).HasName("PK__Owner__819385B8F49B7D56");

            entity.Property(e => e.OwnerId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.OwnerType).HasDefaultValue("PERSON");
        });

        modelBuilder.Entity<PasswordRecovery>(entity =>
        {
            entity.HasKey(e => e.PasswordRecoveryId).HasName("PK__Password__A7DC5AFD77A1EB30");

            entity.HasOne(d => d.User).WithMany(p => p.PasswordRecoveries).HasConstraintName("FK__PasswordR__UserI__498EEC8D");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payment__9B556A3890353B15");

            entity.Property(e => e.CertificateFee).HasDefaultValue(0m);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.ReceiptPrintCount).HasDefaultValue(0);
            entity.Property(e => e.StickerFee).HasDefaultValue(0m);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.PaymentCreatedByNavigations).HasConstraintName("FK__Payment__Created__37FA4C37");

            entity.HasOne(d => d.FeeSchedule).WithMany(p => p.Payments).HasConstraintName("FK__Payment__FeeSche__370627FE");

            entity.HasOne(d => d.Inspection).WithOne(p => p.Payment)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payment__Inspect__361203C5");

            entity.HasOne(d => d.PaidByNavigation).WithMany(p => p.PaymentPaidByNavigations).HasConstraintName("FK__Payment__PaidBy__38EE7070");
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.PositionId).HasName("PK__Position__60BB9A7921591801");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE1A6604636D");
        });

        modelBuilder.Entity<Specification>(entity =>
        {
            entity.HasKey(e => e.SpecificationId).HasName("PK__Specific__A384CDFD7BC535A1");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.HasDriverCamera).HasDefaultValue(false);
            entity.Property(e => e.HasTachograph).HasDefaultValue(false);
            entity.Property(e => e.NotIssuedStamp).HasDefaultValue(false);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.SpecificationCreatedByNavigations).HasConstraintName("FK__Specifica__Creat__318258D2");

            entity.HasOne(d => d.PlateNoNavigation).WithOne(p => p.Specification)
                .HasPrincipalKey<Vehicle>(p => p.PlateNo)
                .HasForeignKey<Specification>(d => d.PlateNo)
                .HasConstraintName("FK__Specifica__Plate__308E3499");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.SpecificationUpdatedByNavigations).HasConstraintName("FK__Specifica__Updat__32767D0B");
        });

        modelBuilder.Entity<Stage>(entity =>
        {
            entity.HasKey(e => e.StageId).HasName("PK__Stage__03EB7AD83E68D7F4");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<StageItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__StageIte__727E838B9040A594");

            entity.Property(e => e.IsRequired).HasDefaultValue(true);

            entity.HasOne(d => d.Stage).WithMany(p => p.StageItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StageItem__Stage__467D75B8");
        });

        modelBuilder.Entity<StageItemThreshold>(entity =>
        {
            entity.HasKey(e => e.ThresholdId).HasName("PK__StageIte__8E87A7D075A9180A");

            entity.Property(e => e.EffectiveDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Item).WithMany(p => p.StageItemThresholds)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StageItem__ItemI__50FB042B");

            entity.HasOne(d => d.VehicleType).WithMany(p => p.StageItemThresholds)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StageItem__Vehic__51EF2864");
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

            entity.HasMany(d => d.Stages).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserStage",
                    r => r.HasOne<Stage>().WithMany()
                        .HasForeignKey("StageId")
                        .HasConstraintName("FK__UserStage__Stage__4C0144E4"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK__UserStage__UserI__4B0D20AB"),
                    j =>
                    {
                        j.HasKey("UserId", "StageId").HasName("PK__UserStag__87B67BE10E21FF63");
                        j.ToTable("UserStage");
                    });
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.VehicleId).HasName("PK__Vehicle__476B549284C6B0C5");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.HasCommercialModification).HasDefaultValue(false);
            entity.Property(e => e.HasModification).HasDefaultValue(false);
            entity.Property(e => e.IsCleanEnergy).HasDefaultValue(false);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.VehicleCreatedByNavigations).HasConstraintName("FK__Vehicle__Created__27F8EE98");

            entity.HasOne(d => d.Owner).WithMany(p => p.Vehicles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Vehicle__OwnerId__2704CA5F");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.VehicleUpdatedByNavigations).HasConstraintName("FK__Vehicle__Updated__28ED12D1");
        });

        modelBuilder.Entity<VehicleType>(entity =>
        {
            entity.HasKey(e => e.VehicleTypeId).HasName("PK__VehicleT__9F44964319AC58C5");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
