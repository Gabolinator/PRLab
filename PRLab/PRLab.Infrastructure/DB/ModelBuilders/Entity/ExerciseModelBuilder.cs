using Microsoft.EntityFrameworkCore;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Infrastructure.DB.ModelBuilders.Entity;

public static class ExerciseModelBuilder
{
    public static void AddExerciseTableModels(this ModelBuilder modelBuilder)
    {
        modelBuilder.CreateExerciseTableModel();
        modelBuilder.CreateExerciseBlockTableModel();
    }

      public static void CreateExerciseTableModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exercise>(exercise =>
        {
            exercise.ToTable("Exercise");

            exercise.HasKey(exercise => exercise.Id);

            exercise.Property(exercise => exercise.Id)
                .HasConversion(
                    exerciseId => exerciseId.Value,
                    value => ExerciseId.FromGuid(value))
                .ValueGeneratedNever();

            exercise.Property(exercise => exercise.Name)
                .HasMaxLength(150)
                .IsRequired();

            exercise.Property(exercise => exercise.NameKey)
                .HasMaxLength(150)
                .IsRequired();

            exercise.HasOne(exercise => exercise.Description)
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            exercise.OwnsOne(exercise => exercise.Audit, audit =>
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
            
            exercise.OwnsOne(exercise => exercise.Ownership, ownership =>
            {
                ownership.Property(ownershipInfo => ownershipInfo.Origin)
                    .HasColumnName("DataOrigin")
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsRequired();

                ownership.Property(ownershipInfo => ownershipInfo.OwnerUserId)
                    .HasColumnName("OwnerUserId")
                    .HasConversion<Guid?>(
                        userId => userId.HasValue ? userId.Value.Value : null,
                        value => value.HasValue ? UserId.FromGuid(value.Value) : null);
                
                ownership.HasIndex(ownershipInfo => ownershipInfo.Origin)
                    .HasDatabaseName("IX_Exercise_DataOrigin");

                ownership.HasIndex(ownershipInfo => ownershipInfo.OwnerUserId)
                    .HasDatabaseName("IX_Exercise_OwnerUserId");
            });

            exercise.Navigation(exercise => exercise.Blocks)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });
    }

    public static void CreateExerciseBlockTableModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ExerciseBlock>(exerciseBlock =>
        {
            exerciseBlock.ToTable("ExerciseBlock");

            exerciseBlock.HasKey(exerciseBlock => exerciseBlock.Id);

            exerciseBlock.Property(exerciseBlock => exerciseBlock.Id)
                .HasConversion(
                    exerciseBlockId => exerciseBlockId.Value,
                    value => ExerciseBlockId.FromGuid(value))
                .ValueGeneratedNever();

            exerciseBlock.Property(exerciseBlock => exerciseBlock.ExerciseId)
                .HasConversion(
                    exerciseId => exerciseId.Value,
                    value => ExerciseId.FromGuid(value))
                .IsRequired();

            exerciseBlock.Property(exerciseBlock => exerciseBlock.MovementId)
                .HasConversion(
                    movementId => movementId.Value,
                    value => MovementId.FromGuid(value))
                .IsRequired();

            exerciseBlock.Property(exerciseBlock => exerciseBlock.Sequence)
                .IsRequired();

            exerciseBlock.HasOne<Exercise>()
                .WithMany(exercise => exercise.Blocks)
                .HasForeignKey(exerciseBlock => exerciseBlock.ExerciseId)
                .OnDelete(DeleteBehavior.Cascade);

            exerciseBlock.HasOne(exerciseBlock => exerciseBlock.Movement)
                .WithMany()
                .HasForeignKey(exerciseBlock => exerciseBlock.MovementId)
                .OnDelete(DeleteBehavior.Restrict);

            exerciseBlock.OwnsOne(exerciseBlock => exerciseBlock.Target, target =>
            {
                target.Property(workTarget => workTarget.Value)
                    .HasPrecision(10, 2)
                    .IsRequired();

                target.Property(workTarget => workTarget.TargetType)
                    .HasConversion<string>()
                    .HasMaxLength(80)
                    .IsRequired();
            });

            exerciseBlock.OwnsOne(exerciseBlock => exerciseBlock.LoadTarget, loadTarget =>
            {
                loadTarget.Property(target => target.Value)
                    .HasPrecision(10, 2);

                loadTarget.Property(target => target.Type)
                    .HasConversion<string>()
                    .HasMaxLength(80)
                    .IsRequired();

                loadTarget.Property(target => target.Unit)
                    .HasConversion<string>()
                    .HasMaxLength(80);
            });

            exerciseBlock.OwnsOne(exerciseBlock => exerciseBlock.RestBetweenReps, restTarget =>
            {
                restTarget.Property(target => target.Seconds);
            });

            exerciseBlock.OwnsOne(exerciseBlock => exerciseBlock.TransitionAfterBlock, restTarget =>
            {
                restTarget.Property(target => target.Seconds);
            });

            exerciseBlock.OwnsOne(exerciseBlock => exerciseBlock.ExecutionDetails, executionDetails =>
            {
                executionDetails.Property(details => details.EccentricSeconds);

                executionDetails.Property(details => details.BottomPauseSeconds);

                executionDetails.Property(details => details.ConcentricSeconds);

                executionDetails.Property(details => details.TopPauseSeconds);

                executionDetails.Property(details => details.EccentricIntent)
                    .HasConversion<string>()
                    .HasMaxLength(80);

                executionDetails.Property(details => details.BottomIntent)
                    .HasConversion<string>()
                    .HasMaxLength(80);

                executionDetails.Property(details => details.ConcentricIntent)
                    .HasConversion<string>()
                    .HasMaxLength(80);

                executionDetails.Property(details => details.TopIntent)
                    .HasConversion<string>()
                    .HasMaxLength(80);

                executionDetails.Property(details => details.Intent)
                    .HasMaxLength(500);
            });
        });
    }

    public static void AddExerciseIndexes(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exercise>(exercise =>
        {
            exercise.HasIndex(exercise => exercise.Name)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");

            exercise.HasIndex(exercise => exercise.NameKey)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");

            exercise.HasIndex("DescriptionId");
        });

        modelBuilder.Entity<ExerciseBlock>(exerciseBlock =>
        {
            exerciseBlock.HasIndex(exerciseBlock => exerciseBlock.ExerciseId);

            exerciseBlock.HasIndex(exerciseBlock => exerciseBlock.MovementId);

            exerciseBlock.HasIndex(exerciseBlock => new
            {
                exerciseBlock.ExerciseId,
                exerciseBlock.Sequence
            })
                .IsUnique();
        });
    }
}