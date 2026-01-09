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
            entity.HasKey(e => e.UserId).HasName("PK__Account__1788CC4C5B1167A6");

            entity.HasIndex(e => e.Username, "UX_Account_Username_NotNull")
                .IsUnique()
                .HasFilter("([Username] IS NOT NULL)");

            entity.Property(e => e.UserId).ValueGeneratedNever();

            entity.HasOne(d => d.User).WithOne(p => p.Account).HasConstraintName("FK_Account_User");
        });

        modelBuilder.Entity<Certificate>(entity =>
        {
            entity.HasKey(e => e.CertificateId).HasName("PK__Certific__BBF8A7C13B05DB2B");

            entity.Property(e => e.IssuedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.PrintCount).HasDefaultValue(0);
            entity.Property(e => e.PrintTemplate).HasDefaultValue("STANDARD");
            entity.Property(e => e.Status).HasDefaultValue((short)1);
            entity.Property(e => e.ValidityMonths).HasDefaultValue(12);

            entity.HasOne(d => d.Inspection).WithOne(p => p.Certificate)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Certifica__Inspe__18B6AB08");

            entity.HasOne(d => d.IssuedByNavigation).WithMany(p => p.Certificates).HasConstraintName("FK__Certifica__Issue__19AACF41");
        });

        modelBuilder.Entity<FeeSchedule>(entity =>
        {
            entity.HasKey(e => e.FeeId).HasName("PK__FeeSched__B387B2295EBD386E");

            entity.Property(e => e.CertificateFee).HasDefaultValue(0m);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.StickerFee).HasDefaultValue(0m);

            entity.HasOne(d => d.VehicleType).WithMany(p => p.FeeSchedules).HasConstraintName("FK__FeeSchedu__Vehic__7E02B4CC");
        });

        modelBuilder.Entity<Inspection>(entity =>
        {
            entity.HasKey(e => e.InspectionId).HasName("PK__Inspecti__30B2DC0814F10BF1");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.InspectionType).HasDefaultValue("FIRST");

            entity.HasOne(d => d.ConcludedByNavigation).WithMany(p => p.InspectionConcludedByNavigations).HasConstraintName("FK__Inspectio__Concl__59C55456");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InspectionCreatedByNavigations).HasConstraintName("FK__Inspectio__Creat__5AB9788F");

            entity.HasOne(d => d.Lane).WithMany(p => p.Inspections).HasConstraintName("FK__Inspectio__LaneI__58D1301D");

            entity.HasOne(d => d.Owner).WithMany(p => p.Inspections)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inspectio__Owner__57DD0BE4");

            entity.HasOne(d => d.ReceivedByNavigation).WithMany(p => p.InspectionReceivedByNavigations).HasConstraintName("FK__Inspectio__Recei__5BAD9CC8");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.Inspections)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inspectio__Vehic__56E8E7AB");
        });

        modelBuilder.Entity<InspectionDefect>(entity =>
        {
            entity.HasKey(e => e.DefectId).HasName("PK__Inspecti__144A379CB0255E4D");

            entity.Property(e => e.Severity).HasDefaultValue(2);

            entity.HasOne(d => d.InspStage).WithMany(p => p.InspectionDefects).HasConstraintName("FK__Inspectio__InspS__74794A92");

            entity.HasOne(d => d.Inspection).WithMany(p => p.InspectionDefects).HasConstraintName("FK__Inspectio__Inspe__73852659");

            entity.HasOne(d => d.Item).WithMany(p => p.InspectionDefects).HasConstraintName("FK__Inspectio__ItemI__756D6ECB");

            entity.HasOne(d => d.VerifiedByNavigation).WithMany(p => p.InspectionDefects).HasConstraintName("FK__Inspectio__Verif__76619304");
        });

        modelBuilder.Entity<InspectionDetail>(entity =>
        {
            entity.HasKey(e => e.DetailId).HasName("PK__Inspecti__135C316DEB057E11");

            entity.Property(e => e.DataSource).HasDefaultValue("MANUAL");
            entity.Property(e => e.RecordedAt).HasDefaultValueSql("(sysdatetime())");

            entity.HasOne(d => d.InspStage).WithMany(p => p.InspectionDetails).HasConstraintName("FK__Inspectio__InspS__6DCC4D03");

            entity.HasOne(d => d.Item).WithMany(p => p.InspectionDetails)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inspectio__ItemI__6EC0713C");
        });

        modelBuilder.Entity<InspectionStage>(entity =>
        {
            entity.HasKey(e => e.InspStageId).HasName("PK__Inspecti__A32B45EA54E1D875");

            entity.Property(e => e.IsRequired).HasDefaultValue(true);

            entity.HasOne(d => d.AssignedUser).WithMany(p => p.InspectionStages).HasConstraintName("FK__Inspectio__Assig__662B2B3B");

            entity.HasOne(d => d.Inspection).WithMany(p => p.InspectionStages).HasConstraintName("FK__Inspectio__Inspe__6442E2C9");

            entity.HasOne(d => d.Stage).WithMany(p => p.InspectionStages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Inspectio__Stage__65370702");
        });

        modelBuilder.Entity<Lane>(entity =>
        {
            entity.HasKey(e => e.LaneId).HasName("PK__Lane__A5770E0C40EAA0BC");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<LaneStage>(entity =>
        {
            entity.HasKey(e => e.LaneStageId).HasName("PK__LaneStag__4A070A395C856C4F");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.IsRequired).HasDefaultValue(true);

            entity.HasOne(d => d.Lane).WithMany(p => p.LaneStages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LaneStage__LaneI__3D2915A8");

            entity.HasOne(d => d.Stage).WithMany(p => p.LaneStages)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__LaneStage__Stage__3E1D39E1");
        });

        modelBuilder.Entity<Owner>(entity =>
        {
            entity.HasKey(e => e.OwnerId).HasName("PK__Owner__819385B8E11FB884");

            entity.HasIndex(e => e.TaxCode, "UX_Owner_TaxCode_Company")
                .IsUnique()
                .HasFilter("([OwnerType]=N'COMPANY' AND [TaxCode] IS NOT NULL)");

            entity.Property(e => e.OwnerId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.OwnerType).HasDefaultValue("PERSON");
        });

        modelBuilder.Entity<PasswordRecovery>(entity =>
        {
            entity.HasKey(e => e.PasswordRecoveryId).HasName("PK__Password__A7DC5AFD7898D539");

            entity.HasOne(d => d.User).WithMany(p => p.PasswordRecoveries).HasConstraintName("FK__PasswordR__UserI__07C12930");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("PK__Payment__9B556A383382DECA");

            entity.Property(e => e.CertificateFee).HasDefaultValue(0m);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.ReceiptPrintCount).HasDefaultValue(0);
            entity.Property(e => e.StickerFee).HasDefaultValue(0m);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.PaymentCreatedByNavigations).HasConstraintName("FK__Payment__Created__0B5CAFEA");

            entity.HasOne(d => d.FeeSchedule).WithMany(p => p.Payments).HasConstraintName("FK__Payment__FeeSche__0A688BB1");

            entity.HasOne(d => d.Inspection).WithOne(p => p.Payment)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Payment__Inspect__09746778");

            entity.HasOne(d => d.PaidByNavigation).WithMany(p => p.PaymentPaidByNavigations).HasConstraintName("FK__Payment__PaidBy__0C50D423");
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.HasKey(e => e.PositionId).HasName("PK__Position__60BB9A7983227232");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__Role__8AFACE1A881C31DC");
        });

        modelBuilder.Entity<Specification>(entity =>
        {
            entity.HasKey(e => e.SpecificationId).HasName("PK__Specific__A384CDFDBCFE2FA0");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.HasDriverCamera).HasDefaultValue(false);
            entity.Property(e => e.HasTachograph).HasDefaultValue(false);
            entity.Property(e => e.NotIssuedStamp).HasDefaultValue(false);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.SpecificationCreatedByNavigations).HasConstraintName("FK__Specifica__Creat__2EDAF651");

            entity.HasOne(d => d.PlateNoNavigation).WithOne(p => p.Specification)
                .HasPrincipalKey<Vehicle>(p => p.PlateNo)
                .HasForeignKey<Specification>(d => d.PlateNo)
                .HasConstraintName("FK__Specifica__Plate__2DE6D218");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.SpecificationUpdatedByNavigations).HasConstraintName("FK__Specifica__Updat__2FCF1A8A");
        });

        modelBuilder.Entity<Stage>(entity =>
        {
            entity.HasKey(e => e.StageId).HasName("PK__Stage__03EB7AD8BD3A182C");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        modelBuilder.Entity<StageItem>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__StageIte__727E838BA19BFC2A");

            entity.Property(e => e.IsRequired).HasDefaultValue(true);

            entity.HasOne(d => d.Stage).WithMany(p => p.StageItems)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StageItem__Stage__43D61337");
        });

        modelBuilder.Entity<StageItemThreshold>(entity =>
        {
            entity.HasKey(e => e.ThresholdId).HasName("PK__StageIte__8E87A7D05D577DE0");

            entity.Property(e => e.EffectiveDate).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);

            entity.HasOne(d => d.Item).WithMany(p => p.StageItemThresholds)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StageItem__ItemI__4E53A1AA");

            entity.HasOne(d => d.VehicleType).WithMany(p => p.StageItemThresholds)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__StageItem__Vehic__4F47C5E3");
        });

        modelBuilder.Entity<Team>(entity =>
        {
            entity.HasKey(e => e.TeamId).HasName("PK__Team__123AE799329C63D8");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.UserId).HasName("PK__User__1788CC4C5082BE11");

            entity.Property(e => e.UserId).HasDefaultValueSql("(newid())");
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(getdate())");
            entity.Property(e => e.IsActive).HasDefaultValue(true);
            entity.Property(e => e.PositionId).HasDefaultValue(1);
            entity.Property(e => e.TeamId).HasDefaultValue(1);

            entity.HasOne(d => d.Position).WithMany(p => p.Users).HasConstraintName("FK__User__PositionId__7B5B524B");

            entity.HasOne(d => d.Team).WithMany(p => p.Users).HasConstraintName("FK__User__TeamId__7C4F7684");

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "User_Role",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .HasConstraintName("FK__User_Role__RoleI__0B91BA14"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK__User_Role__UserI__0A9D95DB"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId").HasName("PK__User_Rol__AF2760AD5C9B09C9");
                        j.ToTable("User_Role");
                    });

            entity.HasMany(d => d.Stages).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserStage",
                    r => r.HasOne<Stage>().WithMany()
                        .HasForeignKey("StageId")
                        .HasConstraintName("FK__UserStage__Stage__2F9A1060"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .HasConstraintName("FK__UserStage__UserI__2EA5EC27"),
                    j =>
                    {
                        j.HasKey("UserId", "StageId").HasName("PK__UserStag__87B67BE1DFA7944D");
                        j.ToTable("UserStage");
                    });
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.VehicleId).HasName("PK__Vehicle__476B54929039405F");

            entity.Property(e => e.CreatedAt).HasDefaultValueSql("(sysdatetime())");
            entity.Property(e => e.HasCommercialModification).HasDefaultValue(false);
            entity.Property(e => e.HasModification).HasDefaultValue(false);
            entity.Property(e => e.IsCleanEnergy).HasDefaultValue(false);

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.VehicleCreatedByNavigations).HasConstraintName("FK__Vehicle__Created__25518C17");

            entity.HasOne(d => d.Owner).WithMany(p => p.Vehicles)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Vehicle__OwnerId__245D67DE");

            entity.HasOne(d => d.UpdatedByNavigation).WithMany(p => p.VehicleUpdatedByNavigations).HasConstraintName("FK__Vehicle__Updated__2645B050");
        });

        modelBuilder.Entity<VehicleType>(entity =>
        {
            entity.HasKey(e => e.VehicleTypeId).HasName("PK__VehicleT__9F449643FDA1C8DC");

            entity.Property(e => e.IsActive).HasDefaultValue(true);
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
