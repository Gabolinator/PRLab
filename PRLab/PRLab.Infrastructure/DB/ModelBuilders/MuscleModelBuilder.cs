using Microsoft.EntityFrameworkCore;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Infrastructure.DB.ModelBuilders;

public static class MuscleModelBuilder
{
    public static void CreateMuscleTableModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Muscle>(muscle =>
        {
            muscle.ToTable("Muscle");

            muscle.HasKey(muscle => muscle.Id);

            muscle.Property(muscle => muscle.Id)
                .HasConversion(
                    muscleId => muscleId.Value,
                    value => MuscleId.FromGuid(value))
                .ValueGeneratedNever();

            muscle.Property(muscle => muscle.Name)
                .HasMaxLength(150)
                .IsRequired();
            
            muscle.Property(muscle => muscle.NameKey)
                .HasMaxLength(150)
                .IsRequired();

            muscle.Property(muscle => muscle.LatinName)
                .HasMaxLength(256);

            muscle.Property(muscle => muscle.BodySection)
                .HasConversion<string>()
                .HasMaxLength(80)
                .IsRequired();

            muscle.HasOne(muscle => muscle.Description)
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            muscle.OwnsOne(muscle => muscle.Audit, audit =>
            {
                audit.Property(auditInfo => auditInfo.CreatedAt)
                    .IsRequired();

                audit.Property(auditInfo => auditInfo.CreatedBy)
                    .HasConversion<Guid>(
                        userId => userId.Value,
                        value => UserId.FromGuid(value))
                    .IsRequired();

                audit.Property(auditInfo => auditInfo.UpdatedAt);

                audit.Property(auditInfo => auditInfo.UpdatedBy)
                    .HasConversion<Guid?>(
                        userId => userId.HasValue ? userId.Value.Value : null,
                        value => value.HasValue ? UserId.FromGuid(value.Value) : null);

                audit.Property(auditInfo => auditInfo.IsDeleted)
                    .HasColumnName("IsDeleted")
                    .IsRequired();

                audit.HasIndex(auditInfo => auditInfo.IsDeleted);

                audit.Property(auditInfo => auditInfo.DeletedAt);

                audit.Property(auditInfo => auditInfo.DeletedBy)
                    .HasConversion<Guid?>(
                        userId => userId.HasValue ? userId.Value.Value : null,
                        value => value.HasValue ? UserId.FromGuid(value.Value) : null);
            });
        });
    }

    public static void CreateMuscleAntagonistTableModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MuscleAntagonist>(muscleAntagonist =>
        {
            muscleAntagonist.ToTable("MuscleAntagonist");

            muscleAntagonist.HasKey(muscleAntagonist => new
            {
                muscleAntagonist.MuscleId,
                muscleAntagonist.AntagonistMuscleId
            });

            muscleAntagonist.Property(muscleAntagonist => muscleAntagonist.MuscleId)
                .HasConversion(
                    muscleId => muscleId.Value,
                    value => MuscleId.FromGuid(value))
                .IsRequired();

            muscleAntagonist.Property(muscleAntagonist => muscleAntagonist.AntagonistMuscleId)
                .HasConversion(
                    muscleId => muscleId.Value,
                    value => MuscleId.FromGuid(value))
                .IsRequired();

            muscleAntagonist.HasOne(muscleAntagonist => muscleAntagonist.Muscle)
                .WithMany(muscle => muscle.Antagonists)
                .HasForeignKey(muscleAntagonist => muscleAntagonist.MuscleId)
                .OnDelete(DeleteBehavior.Cascade);

            muscleAntagonist.HasOne(muscleAntagonist => muscleAntagonist.AntagonistMuscle)
                .WithMany()
                .HasForeignKey(muscleAntagonist => muscleAntagonist.AntagonistMuscleId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Muscle>(muscle =>
        {
            muscle.Navigation(muscle => muscle.Antagonists)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });
    }

    public static void AddMuscleIndexes(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Muscle>(muscle =>
        {
            muscle.HasIndex(muscle => muscle.Name)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");
            
            muscle.HasIndex(muscle => muscle.NameKey)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");

            muscle.HasIndex(muscle => muscle.LatinName);

            muscle.HasIndex(muscle => muscle.BodySection);

            muscle.HasIndex("DescriptionId");
        });

        modelBuilder.Entity<MuscleAntagonist>(muscleAntagonist =>
        {
            muscleAntagonist.HasIndex(muscleAntagonist => muscleAntagonist.AntagonistMuscleId);
        });
    }
}