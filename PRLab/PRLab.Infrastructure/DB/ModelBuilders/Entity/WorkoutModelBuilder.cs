using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Model.Value;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Ownership;
using PRLab.Domain.Model.Value.Prescription.Common;
using PRLab.Domain.Model.Value.Prescription.Intensity;
using PRLab.Domain.Model.Value.Prescription.Load;
using PRLab.Domain.Model.Value.Prescription.Rest;
using PRLab.Domain.Model.Value.Prescription.Time;
using PRLab.Domain.Model.Value.Prescription.Work;
using PRLab.Domain.Model.Value.Prescription.Workout;
using PRLab.Domain.Model.Value.WorkoutValue;

namespace PRLab.Infrastructure.DB.ModelBuilders.Entity;

public static class WorkoutModelBuilder
{
    public static void AddWorkoutTableModels(this ModelBuilder modelBuilder)
    {
        modelBuilder.CreateWorkoutTableModel();
        modelBuilder.CreateWorkoutBlockTableModel();
        modelBuilder.CreateWorkoutBlockAssignmentTableModel();
        modelBuilder.CreateWorkoutBlockSegmentTableModel();
        modelBuilder.CreateWorkoutBlockSegmentStepTableModel();
    }

    public static void CreateWorkoutTableModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Workout>(workout =>
        {
            workout.ToTable("Workout");

            workout.HasKey(workout => workout.Id);

            workout.Property(workout => workout.Id)
                .HasConversion(
                    workoutId => workoutId.Value,
                    value => WorkoutId.FromGuid(value))
                .ValueGeneratedNever();

            workout.Property(workout => workout.Name)
                .HasMaxLength(200)
                .IsRequired();

            workout.Property(workout => workout.NameKey)
                .HasMaxLength(200)
                .IsRequired();

            workout.HasOne(workout => workout.Description)
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            MapEstimatedDuration(
                workout,
                workout => workout.EstimatedDuration,
                "EstimatedDuration");

            MapAudit(
                workout,
                workout => workout.Audit,
                "Workout");

            MapOwnership(
                workout,
                workout => workout.Ownership,
                "Workout");

            workout.HasMany(workout => workout.Blocks)
                .WithOne()
                .HasForeignKey(assignment => assignment.WorkoutId)
                .OnDelete(DeleteBehavior.Cascade);

            workout.Navigation(workout => workout.Blocks)
                .HasField("blocks")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });
    }

    public static void CreateWorkoutBlockAssignmentTableModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkoutBlockAssignment>(assignment =>
        {
            assignment.ToTable("WorkoutBlockAssignment");

            assignment.HasKey(assignment => assignment.Id);

            assignment.Property(assignment => assignment.Id)
                .HasConversion(
                    assignmentId => assignmentId.Value,
                    value => WorkoutBlockAssignmentId.FromGuid(value))
                .ValueGeneratedNever();

            assignment.Property(assignment => assignment.WorkoutId)
                .HasConversion(
                    workoutId => workoutId.Value,
                    value => WorkoutId.FromGuid(value))
                .IsRequired();

            assignment.Property(assignment => assignment.WorkoutBlockId)
                .HasConversion(
                    workoutBlockId => workoutBlockId.Value,
                    value => WorkoutBlockId.FromGuid(value))
                .IsRequired();

            assignment.Property(assignment => assignment.Sequence)
                .IsRequired();

            assignment.HasOne(assignment => assignment.WorkoutBlock)
                .WithMany()
                .HasForeignKey(assignment => assignment.WorkoutBlockId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public static void CreateWorkoutBlockTableModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkoutBlock>(workoutBlock =>
        {
            workoutBlock.ToTable("WorkoutBlock");

            workoutBlock.HasKey(workoutBlock => workoutBlock.Id);

            workoutBlock.Property(workoutBlock => workoutBlock.Id)
                .HasConversion(
                    workoutBlockId => workoutBlockId.Value,
                    value => WorkoutBlockId.FromGuid(value))
                .ValueGeneratedNever();

            workoutBlock.Property(workoutBlock => workoutBlock.Name)
                .HasMaxLength(200)
                .IsRequired();

            workoutBlock.Property(workoutBlock => workoutBlock.NameKey)
                .HasMaxLength(200)
                .IsRequired();

            workoutBlock.Property(workoutBlock => workoutBlock.BlockType)
                .HasConversion<string>()
                .HasMaxLength(80)
                .IsRequired();

            workoutBlock.OwnsOne(workoutBlock => workoutBlock.BlockRepeatPrescription, repeat =>
            {
                repeat.Property(prescription => prescription.RepeatCount)
                    .HasColumnName("RepeatCount")
                    .IsRequired();

                repeat.Property(prescription => prescription.PrepareTime)
                    .HasColumnName("PrepareTime");

                MapRestTarget(
                    repeat,
                    prescription => prescription.RestBetweenRepeats,
                    "RestBetweenRepeats");

                MapRestTarget(
                    repeat,
                    prescription => prescription.RestAfterBlock,
                    "RestAfterBlock");

                MapEstimatedDuration(
                    repeat,
                    prescription => prescription.EstimatedRepeatDuration,
                    "EstimatedRepeatDuration");
            });

            MapAudit(
                workoutBlock,
                workoutBlock => workoutBlock.Audit,
                "WorkoutBlock");

            MapOwnership(
                workoutBlock,
                workoutBlock => workoutBlock.Ownership,
                "WorkoutBlock");

            workoutBlock.HasMany(workoutBlock => workoutBlock.Segments)
                .WithOne()
                .HasForeignKey(segment => segment.WorkoutBlockId)
                .OnDelete(DeleteBehavior.Cascade);

            workoutBlock.Navigation(workoutBlock => workoutBlock.Segments)
                .HasField("segments")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });
    }

    public static void CreateWorkoutBlockSegmentTableModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkoutBlockSegment>(segment =>
        {
            segment.ToTable("WorkoutBlockSegment");

            segment.HasKey(segment => segment.Id);

            segment.Property(segment => segment.Id)
                .HasConversion(
                    segmentId => segmentId.Value,
                    value => WorkoutBlockSegmentId.FromGuid(value))
                .ValueGeneratedNever();

            segment.Property(segment => segment.WorkoutBlockId)
                .HasConversion(
                    workoutBlockId => workoutBlockId.Value,
                    value => WorkoutBlockId.FromGuid(value))
                .IsRequired();

            segment.Property(segment => segment.Name)
                .HasMaxLength(200)
                .IsRequired();

            segment.Property(segment => segment.Sequence)
                .IsRequired();

            segment.Property(segment => segment.WorkMode)
                .HasConversion<string>()
                .HasMaxLength(80)
                .IsRequired();

            segment.Property(segment => segment.ScoreType)
                .HasConversion<string>()
                .HasMaxLength(80)
                .IsRequired();

            segment.OwnsOne(segment => segment.Intent, intent =>
            {
                intent.Property(prescription => prescription.WorkIntent)
                    .HasColumnName("Intent")
                    .HasConversion<string>()
                    .HasMaxLength(80)
                    .IsRequired();

                MapTargetIntensity(
                    intent,
                    prescription => prescription.TargetIntensity,
                    "IntentTargetIntensity");
            });

            MapTimeConstraint(
                segment,
                segment => segment.TimeConstraint,
                "TimeConstraint");

            MapIntervalPrescription(
                segment,
                segment => segment.IntervalPrescription,
                "Interval");

            MapEstimatedDuration(
                segment,
                segment => segment.EstimatedSegmentDuration,
                "EstimatedSegmentDuration");

            MapRestTarget(
                segment,
                segment => segment.RestAfterStep,
                "RestAfterStep");

            MapRestTarget(
                segment,
                segment => segment.RestAfterSegment,
                "RestAfterSegment");

            segment.HasMany(segment => segment.Steps)
                .WithOne()
                .HasForeignKey(step => step.SegmentId)
                .OnDelete(DeleteBehavior.Cascade);

            segment.Navigation(segment => segment.Steps)
                .HasField("steps")
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });
    }

    public static void CreateWorkoutBlockSegmentStepTableModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<WorkoutBlockSegmentStep>(step =>
        {
            step.ToTable("WorkoutBlockSegmentStep");

            step.HasKey(step => step.Id);

            step.Property(step => step.Id)
                .HasConversion(
                    stepId => stepId.Value,
                    value => WorkoutBlockSegmentStepId.FromGuid(value))
                .ValueGeneratedNever();

            step.Property(step => step.SegmentId)
                .HasConversion(
                    segmentId => segmentId.Value,
                    value => WorkoutBlockSegmentId.FromGuid(value))
                .IsRequired();

            step.Property(step => step.ExerciseId)
                .HasConversion<Guid?>(
                    exerciseId => exerciseId.HasValue ? exerciseId.Value.Value : null,
                    value => value.HasValue ? ExerciseId.FromGuid(value.Value) : null);

            step.Property(step => step.StepKind)
                .HasConversion<string>()
                .HasMaxLength(80)
                .IsRequired();

            step.Property(step => step.Sequence)
                .IsRequired();

            step.Property(step => step.Notes)
                .HasMaxLength(1000);

            step.HasOne(step => step.Exercise)
                .WithMany()
                .HasForeignKey(step => step.ExerciseId)
                .OnDelete(DeleteBehavior.Restrict);

            MapRestTarget(
                step,
                step => step.Rest,
                "Rest");

            step.OwnsOne(step => step.Prescription, prescription =>
            {
                prescription.ToJson("Prescription");

                prescription.OwnsOne(value => value.WorkTarget, workTarget =>
                {
                    workTarget.Property(value => value.Value)
                        .HasPrecision(18, 4);

                    workTarget.Property(value => value.TargetType)
                        .HasConversion<string>();

                    workTarget.Property(value => value.Scope)
                        .HasConversion<string>();
                });

                prescription.OwnsOne(value => value.LoadTarget, loadTarget =>
                {
                    loadTarget.Property(value => value.Value)
                        .HasPrecision(18, 4);

                    loadTarget.Property(value => value.Type)
                        .HasConversion<string>();

                    loadTarget.Property(value => value.Unit)
                        .HasConversion<string>();

                    loadTarget.Property(value => value.ReferenceRepMax);

                    loadTarget.OwnsOne(value => value.LoadReference, loadReference =>
                    {
                        loadReference.Property(value => value.Kind)
                            .HasConversion<string>();

                        loadReference.Property(value => value.ExerciseId)
                            .HasConversion(
                                exerciseId => exerciseId.HasValue
                                    ? exerciseId.Value.Value
                                    : (Guid?)null,
                                value => value.HasValue
                                    ? ExerciseId.FromGuid(value.Value)
                                    : null);

                        loadReference.Property(value => value.MovementId)
                            .HasConversion(
                                movementId => movementId.HasValue
                                    ? movementId.Value.Value
                                    : (Guid?)null,
                                value => value.HasValue
                                    ? MovementId.FromGuid(value.Value)
                                    : null);

                        loadReference.Property(value => value.Name);
                    });
                });

                prescription.OwnsOne(value => value.RestAfterStep, rest =>
                {
                    rest.Property(value => value.Policy)
                        .HasConversion<string>();

                    rest.Property(value => value.Seconds);
                    rest.Property(value => value.MinimumSeconds);
                    rest.Property(value => value.MaximumSeconds);
                });

                prescription.OwnsOne(value => value.TimeConstraint, timeConstraint =>
                {
                    timeConstraint.Property(value => value.Kind)
                        .HasConversion<string>();

                    timeConstraint.Property(value => value.Duration);
                });

                prescription.OwnsOne(value => value.EstimatedStepDuration, estimatedDuration =>
                {
                    estimatedDuration.Property(value => value.Expected);
                    estimatedDuration.Property(value => value.Minimum);
                    estimatedDuration.Property(value => value.Maximum);
                });

                prescription.OwnsOne(value => value.IntentOverride, intent =>
                {
                    intent.Property(value => value.WorkIntent)
                        .HasConversion<string>();

                    intent.OwnsOne(value => value.TargetIntensity, intensity =>
                    {
                        intensity.Property(value => value.Type)
                            .HasConversion<string>();

                        intensity.Property(value => value.Value)
                            .HasPrecision(18, 4);

                        intensity.OwnsOne(value => value.Range, range =>
                        {
                            range.Property(value => value.MinValue)
                                .HasPrecision(18, 4);

                            range.Property(value => value.MaxValue)
                                .HasPrecision(18, 4);
                        });

                        intensity.OwnsOne(value => value.PaceTarget, pace =>
                        {
                            pace.Property(value => value.Unit)
                                .HasConversion<string>();

                            pace.Property(value => value.Duration);
                        });
                    });
                });

                prescription.OwnsOne(value => value.Partition, partition =>
                {
                    partition.Property(value => value.Strategy)
                        .HasConversion<string>();

                    partition.Property(value => value.RepeatCount);

                    partition.OwnsOne(value => value.RestBetweenRepeats, rest =>
                    {
                        rest.Property(value => value.Policy)
                            .HasConversion<string>();

                        rest.Property(value => value.Seconds);
                        rest.Property(value => value.MinimumSeconds);
                        rest.Property(value => value.MaximumSeconds);
                    });

                    partition.OwnsMany(value => value.RepeatDetails, repeat =>
                    {
                        repeat.Property(value => value.Sequence);

                        repeat.OwnsOne(value => value.WorkTarget, workTarget =>
                        {
                            workTarget.Property(value => value.Value)
                                .HasPrecision(18, 4);

                            workTarget.Property(value => value.TargetType)
                                .HasConversion<string>();

                            workTarget.Property(value => value.Scope)
                                .HasConversion<string>();
                        });

                        repeat.OwnsOne(value => value.LoadTarget, loadTarget =>
                        {
                            loadTarget.Property(value => value.Value)
                                .HasPrecision(18, 4);

                            loadTarget.Property(value => value.Type)
                                .HasConversion<string>();

                            loadTarget.Property(value => value.Unit)
                                .HasConversion<string>();

                            loadTarget.Property(value => value.ReferenceRepMax);

                            loadTarget.OwnsOne(value => value.LoadReference, loadReference =>
                            {
                                loadReference.Property(value => value.Kind)
                                    .HasConversion<string>();

                                loadReference.Property(value => value.ExerciseId)
                                    .HasConversion(
                                        exerciseId => exerciseId.HasValue
                                            ? exerciseId.Value.Value
                                            : (Guid?)null,
                                        value => value.HasValue
                                            ? ExerciseId.FromGuid(value.Value)
                                            : null);

                                loadReference.Property(value => value.MovementId)
                                    .HasConversion(
                                        movementId => movementId.HasValue
                                            ? movementId.Value.Value
                                            : (Guid?)null,
                                        value => value.HasValue
                                            ? MovementId.FromGuid(value.Value)
                                            : null);

                                loadReference.Property(value => value.Name);
                            });
                        });

                        repeat.OwnsOne(value => value.TargetIntensity, intensity =>
                        {
                            intensity.Property(value => value.Type)
                                .HasConversion<string>();

                            intensity.Property(value => value.Value)
                                .HasPrecision(18, 4);

                            intensity.OwnsOne(value => value.Range, range =>
                            {
                                range.Property(value => value.MinValue)
                                    .HasPrecision(18, 4);

                                range.Property(value => value.MaxValue)
                                    .HasPrecision(18, 4);
                            });

                            intensity.OwnsOne(value => value.PaceTarget, pace =>
                            {
                                pace.Property(value => value.Unit)
                                    .HasConversion<string>();

                                pace.Property(value => value.Duration);
                            });
                        });

                        repeat.OwnsOne(value => value.RestAfterRepeat, rest =>
                        {
                            rest.Property(value => value.Policy)
                                .HasConversion<string>();

                            rest.Property(value => value.Seconds);
                            rest.Property(value => value.MinimumSeconds);
                            rest.Property(value => value.MaximumSeconds);
                        });

                        repeat.Property(value => value.Notes);
                    });
                });

                prescription.Property(value => value.SideExecution)
                    .HasConversion<string>();

                prescription.Property(value => value.Notes);
            });
        });
    }

    public static void AddWorkoutIndexes(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Workout>(workout =>
        {
            workout.HasIndex(workout => workout.NameKey);

            workout.HasIndex("DescriptionId");
        });

        modelBuilder.Entity<WorkoutBlockAssignment>(assignment =>
        {
            assignment.HasIndex(assignment => assignment.WorkoutId);

            assignment.HasIndex(assignment => assignment.WorkoutBlockId);

            assignment.HasIndex(assignment => new
                {
                    assignment.WorkoutId,
                    assignment.Sequence
                })
                .IsUnique();
        });

        modelBuilder.Entity<WorkoutBlock>(workoutBlock =>
        {
            workoutBlock.HasIndex(workoutBlock => workoutBlock.NameKey);

            workoutBlock.HasIndex(workoutBlock => workoutBlock.BlockType);
        });

        modelBuilder.Entity<WorkoutBlockSegment>(segment =>
        {
            segment.HasIndex(segment => segment.WorkoutBlockId);

            segment.HasIndex(segment => segment.WorkMode);

            segment.HasIndex(segment => segment.ScoreType);

            segment.HasIndex(segment => new
                {
                    segment.WorkoutBlockId,
                    segment.Sequence
                })
                .IsUnique();
        });

        modelBuilder.Entity<WorkoutBlockSegmentStep>(step =>
        {
            step.HasIndex(step => step.SegmentId);

            step.HasIndex(step => step.ExerciseId);

            step.HasIndex(step => step.StepKind);

            step.HasIndex(step => new
                {
                    step.SegmentId,
                    step.Sequence
                })
                .IsUnique();
        });
    }

    private static void MapAudit<TEntity>(
        EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, AuditInfo>> navigation,
        string indexPrefix)
        where TEntity : class
    {
        builder.OwnsOne(navigation, audit =>
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

            audit.HasIndex(auditInfo => auditInfo.IsDeleted)
                .HasDatabaseName($"IX_{indexPrefix}_IsDeleted");

            audit.Property(auditInfo => auditInfo.DeletedAt);

            audit.Property(auditInfo => auditInfo.DeletedBy)
                .HasConversion<Guid?>(
                    userId => userId.HasValue ? userId.Value.Value : null,
                    value => value.HasValue ? UserId.FromGuid(value.Value) : null);
        });
    }

    private static void MapOwnership<TEntity>(
        EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, OwnershipInfo>> navigation,
        string indexPrefix)
        where TEntity : class
    {
        builder.OwnsOne(navigation, ownership =>
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
                .HasDatabaseName($"IX_{indexPrefix}_DataOrigin");

            ownership.HasIndex(ownershipInfo => ownershipInfo.OwnerUserId)
                .HasDatabaseName($"IX_{indexPrefix}_OwnerUserId");
        });
    }

    private static void MapEstimatedDuration<TEntity>(
        EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, EstimatedDuration?>> navigation,
        string prefix)
        where TEntity : class
    {
        builder.OwnsOne(navigation, duration =>
        {
            duration.Property(value => value.Expected)
                .HasColumnName($"{prefix}Expected");

            duration.Property(value => value.Minimum)
                .HasColumnName($"{prefix}Minimum");

            duration.Property(value => value.Maximum)
                .HasColumnName($"{prefix}Maximum");
        });
    }

    private static void MapEstimatedDuration<TOwner, TEntity>(
        OwnedNavigationBuilder<TOwner, TEntity> builder,
        Expression<Func<TEntity, EstimatedDuration?>> navigation,
        string prefix)
        where TOwner : class
        where TEntity : class
    {
        builder.OwnsOne(navigation, duration =>
        {
            duration.Property(value => value.Expected)
                .HasColumnName($"{prefix}Expected");

            duration.Property(value => value.Minimum)
                .HasColumnName($"{prefix}Minimum");

            duration.Property(value => value.Maximum)
                .HasColumnName($"{prefix}Maximum");
        });
    }

    private static void MapRestTarget<TEntity>(
        EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, RestTarget?>> navigation,
        string prefix)
        where TEntity : class
    {
        builder.OwnsOne(navigation, rest =>
        {
            rest.Property(value => value.Policy)
                .HasColumnName($"{prefix}Policy")
                .HasConversion<string>()
                .HasMaxLength(80);

            rest.Property(value => value.Seconds)
                .HasColumnName($"{prefix}Seconds");

            rest.Property(value => value.MinimumSeconds)
                .HasColumnName($"{prefix}MinimumSeconds");

            rest.Property(value => value.MaximumSeconds)
                .HasColumnName($"{prefix}MaximumSeconds");
        });
    }

    private static void MapRestTarget<TOwner, TEntity>(
        OwnedNavigationBuilder<TOwner, TEntity> builder,
        Expression<Func<TEntity, RestTarget?>> navigation,
        string prefix)
        where TOwner : class
        where TEntity : class
    {
        builder.OwnsOne(navigation, rest =>
        {
            rest.Property(value => value.Policy)
                .HasColumnName($"{prefix}Policy")
                .HasConversion<string>()
                .HasMaxLength(80);

            rest.Property(value => value.Seconds)
                .HasColumnName($"{prefix}Seconds");

            rest.Property(value => value.MinimumSeconds)
                .HasColumnName($"{prefix}MinimumSeconds");

            rest.Property(value => value.MaximumSeconds)
                .HasColumnName($"{prefix}MaximumSeconds");
        });
    }

    private static void MapTimeConstraint<TEntity>(
        EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, TimeConstraint?>> navigation,
        string prefix)
        where TEntity : class
    {
        builder.OwnsOne(navigation, timeConstraint =>
        {
            timeConstraint.Property(value => value.Kind)
                .HasColumnName($"{prefix}Kind")
                .HasConversion<string>()
                .HasMaxLength(80);

            timeConstraint.Property(value => value.Duration)
                .HasColumnName($"{prefix}Duration");
        });
    }

    private static void MapTimeConstraint<TOwner, TEntity>(
        OwnedNavigationBuilder<TOwner, TEntity> builder,
        Expression<Func<TEntity, TimeConstraint?>> navigation,
        string prefix)
        where TOwner : class
        where TEntity : class
    {
        builder.OwnsOne(navigation, timeConstraint =>
        {
            timeConstraint.Property(value => value.Kind)
                .HasColumnName($"{prefix}Kind")
                .HasConversion<string>()
                .HasMaxLength(80);

            timeConstraint.Property(value => value.Duration)
                .HasColumnName($"{prefix}Duration");
        });
    }

    private static void MapIntervalPrescription<TEntity>(
        EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, IntervalPrescription?>> navigation,
        string prefix)
        where TEntity : class
    {
        builder.OwnsOne(navigation, interval =>
        {
            interval.Property(value => value.Duration)
                .HasColumnName($"{prefix}Duration");

            interval.Property(value => value.Scope)
                .HasColumnName($"{prefix}Scope")
                .HasConversion<string>()
                .HasMaxLength(80);

            interval.Property(value => value.StartsOnClock)
                .HasColumnName($"{prefix}StartsOnClock");
        });
    }

    private static void MapWorkTarget<TOwner, TEntity>(
        OwnedNavigationBuilder<TOwner, TEntity> builder,
        Expression<Func<TEntity, WorkTarget?>> navigation,
        string prefix)
        where TOwner : class
        where TEntity : class
    {
        builder.OwnsOne(navigation, workTarget =>
        {
            workTarget.Property(value => value.Value)
                .HasColumnName($"{prefix}Value")
                .HasPrecision(18, 4);

            workTarget.Property(value => value.TargetType)
                .HasColumnName($"{prefix}Type")
                .HasConversion<string>()
                .HasMaxLength(80);

            workTarget.Property(value => value.Scope)
                .HasColumnName($"{prefix}Scope")
                .HasConversion<string>()
                .HasMaxLength(80);
        });
    }

    private static void MapLoadTarget<TOwner, TEntity>(
        OwnedNavigationBuilder<TOwner, TEntity> builder,
        Expression<Func<TEntity, LoadTarget?>> navigation,
        string prefix)
        where TOwner : class
        where TEntity : class
    {
        builder.OwnsOne(navigation, loadTarget =>
        {
            loadTarget.Property(value => value.Value)
                .HasColumnName($"{prefix}Value")
                .HasPrecision(18, 4);

            loadTarget.Property(value => value.Type)
                .HasColumnName($"{prefix}Type")
                .HasConversion<string>()
                .HasMaxLength(80)
                .IsRequired();

            loadTarget.Property(value => value.Unit)
                .HasColumnName($"{prefix}Unit")
                .HasConversion<string>()
                .HasMaxLength(80);

            loadTarget.Property(value => value.ReferenceRepMax)
                .HasColumnName($"{prefix}ReferenceRepMax");

            loadTarget.OwnsOne(value => value.LoadReference, reference =>
            {
                reference.Property(value => value.Kind)
                    .HasColumnName($"{prefix}ReferenceKind")
                    .HasConversion<string>()
                    .HasMaxLength(80);

                reference.Property(value => value.ExerciseId)
                    .HasColumnName($"{prefix}ReferenceExerciseId")
                    .HasConversion(
                        exerciseId => exerciseId.HasValue
                            ? exerciseId.Value.Value
                            : (Guid?)null,
                        value => value.HasValue
                            ? ExerciseId.FromGuid(value.Value)
                            : null);
                
                reference.Property(value => value.MovementId)
                    .HasColumnName($"{prefix}ReferenceMovementId")
                    .HasConversion(
                        movementId => movementId.HasValue
                            ? movementId.Value.Value
                            : (Guid?)null,
                        value => value.HasValue
                            ? MovementId.FromGuid(value.Value)
                            : null);
                
                reference.Property(value => value.Name)
                    .HasColumnName($"{prefix}ReferenceName")
                    .HasMaxLength(200);
            });
        });
    }

    private static void MapWorkIntentPrescription<TOwner, TEntity>(
        OwnedNavigationBuilder<TOwner, TEntity> builder,
        Expression<Func<TEntity, WorkIntentPrescription?>> navigation,
        string prefix)
        where TOwner : class
        where TEntity : class
    {
        builder.OwnsOne(navigation, intent =>
        {
            intent.Property(value => value.WorkIntent)
                .HasColumnName($"{prefix}WorkIntent")
                .HasConversion<string>()
                .HasMaxLength(80)
                .IsRequired();

            MapTargetIntensity(
                intent,
                value => value.TargetIntensity,
                $"{prefix}TargetIntensity");
        });
    }

    private static void MapTargetIntensity<TOwner, TEntity>(
        OwnedNavigationBuilder<TOwner, TEntity> builder,
        Expression<Func<TEntity, TargetIntensity?>> navigation,
        string prefix)
        where TOwner : class
        where TEntity : class
    {
        builder.OwnsOne(navigation, intensity =>
        {
            intensity.Property(value => value.Type)
                .HasColumnName($"{prefix}Type")
                .HasConversion<string>()
                .HasMaxLength(80)
                .IsRequired();

            intensity.Property(value => value.Value)
                .HasColumnName($"{prefix}Value")
                .HasPrecision(18, 4);

            intensity.OwnsOne(value => value.Range, range =>
            {
                range.Property(value => value.MinValue)
                    .HasColumnName($"{prefix}RangeMinValue")
                    .HasPrecision(18, 4);

                range.Property(value => value.MaxValue)
                    .HasColumnName($"{prefix}RangeMaxValue")
                    .HasPrecision(18, 4);
            });

            intensity.OwnsOne(value => value.PaceTarget, pace =>
            {
                pace.Property(value => value.Unit)
                    .HasColumnName($"{prefix}PaceUnit")
                    .HasConversion<string>()
                    .HasMaxLength(80);

                pace.Property(value => value.Duration)
                    .HasColumnName($"{prefix}PaceDuration");
            });
        });
    }

    private static void MapWorkPartitionPrescription<TOwner, TEntity>(
        OwnedNavigationBuilder<TOwner, TEntity> builder,
        Expression<Func<TEntity, WorkPartitionPrescription?>> navigation,
        string prefix)
        where TOwner : class
        where TEntity : class
    {
        builder.OwnsOne(navigation, partition =>
        {
            const string stepId = "WorkoutBlockSegmentStepId";

            partition.ToTable("WorkoutStepPartitionPrescription");

            partition.WithOwner()
                .HasForeignKey(stepId);

            partition.Property<WorkoutBlockSegmentStepId>(stepId)
                .HasColumnName(stepId)
                .HasConversion(
                    workoutBlockSegmentStepId => workoutBlockSegmentStepId.Value,
                    value => WorkoutBlockSegmentStepId.FromGuid(value));

            partition.Property(value => value.Strategy)
                .HasColumnName($"{prefix}Strategy")
                .HasConversion<string>()
                .HasMaxLength(80)
                .IsRequired();

            partition.Property(value => value.RepeatCount)
                .HasColumnName($"{prefix}RepeatCount");

            MapRestTarget(
                partition,
                value => value.RestBetweenRepeats,
                $"{prefix}RestBetweenRepeats");

            partition.OwnsMany(value => value.RepeatDetails, repeat =>
            {
                repeat.ToTable("WorkoutStepRepeatDetail");

                repeat.WithOwner()
                    .HasForeignKey(stepId);

                repeat.Property<WorkoutBlockSegmentStepId>(stepId)
                    .HasColumnName(stepId)
                    .HasConversion(
                        workoutBlockSegmentStepId => workoutBlockSegmentStepId.Value,
                        value => WorkoutBlockSegmentStepId.FromGuid(value));

                repeat.Property(value => value.Sequence)
                    .IsRequired();

                repeat.HasKey(stepId, nameof(WorkRepeatPrescription.Sequence));

                MapWorkTarget(
                    repeat,
                    value => value.WorkTarget,
                    "WorkTarget");

                MapLoadTarget(
                    repeat,
                    value => value.LoadTarget,
                    "LoadTarget");

                MapTargetIntensity(
                    repeat,
                    value => value.TargetIntensity,
                    "TargetIntensity");

                MapRestTarget(
                    repeat,
                    value => value.RestAfterRepeat,
                    "RestAfterRepeat");

                repeat.Property(value => value.Notes)
                    .HasMaxLength(1000);
            });
        });

    }
}