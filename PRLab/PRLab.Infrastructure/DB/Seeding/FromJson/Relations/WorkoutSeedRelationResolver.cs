using PRLab.Application.Models.DB.Seeding.Catalog;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Model.Value.Enum.Prescription.Intensity;
using PRLab.Domain.Model.Value.Enum.Prescription.Load;
using PRLab.Domain.Model.Value.Enum.Prescription.Rest;
using PRLab.Domain.Model.Value.Enum.Prescription.Time;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Enum.Workout;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Prescription.Common;
using PRLab.Domain.Model.Value.Prescription.Intensity;
using PRLab.Domain.Model.Value.Prescription.Load;
using PRLab.Domain.Model.Value.Prescription.Rest;
using PRLab.Domain.Model.Value.Prescription.Time;
using PRLab.Domain.Model.Value.Prescription.Work;
using PRLab.Domain.Model.Value.Prescription.Workout;
using PRLab.Domain.Model.Value.WorkoutStructure;
using PRLab.Domain.Model.Value.WorkoutValue;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Prescription;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Structure;
using PRLab.Infrastructure.DB.Seeding.FromJson.Relations.Interface;
using PRLab.Infrastructure.DB.Seeding.Validation;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Relations;

public sealed class WorkoutSeedRelationResolver : IWorkoutSeedRelationResolver
{
    public void ApplyRelations(
        Workout workout,
        WorkoutSeedJsonDto seedDto,
        ExerciseSeedCatalog catalog,
        SeedExecutionOptions options,
        User seedUser)
    {
        ArgumentNullException.ThrowIfNull(workout);
        ArgumentNullException.ThrowIfNull(seedDto);
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(seedUser);

        if (seedDto.Blocks.Count == 0)
        {
            throw new InvalidOperationException(
                $"Workout seed '{seedDto.Name}' must provide at least one block.");
        }

        WorkoutSeedValidator.ValidateBlockSequences(seedDto);

        foreach (var assignmentDto in seedDto.Blocks.OrderBy(block => block.Sequence))
        {
            var block = ToWorkoutBlock(
                options,
                assignmentDto.Block,
                seedDto.Name,
                seedDto.Origin,
                assignmentDto.Sequence,
                catalog,
                seedUser);

            workout.AddBlock(
                WorkoutBlockAssignment.New(
                    workoutId: workout.Id,
                    workoutBlock: block,
                    sequence: assignmentDto.Sequence));
        }
    }

    private WorkoutBlock ToWorkoutBlock(
        SeedExecutionOptions options,
        WorkoutBlockSeedJsonDto blockDto,
        string workoutName,
        DataOrigin workoutOrigin,
        int blockSequence,
        ExerciseSeedCatalog catalog,
        User seedUser)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(blockDto);

        WorkoutBlockSeedValidator.Validate(
            blockDto,
            workoutName,
            workoutOrigin,
            blockSequence);

        var repeatPrescription = ToBlockRepeatPrescription(
            blockDto.BlockRepeatPrescription,
            workoutName,
            blockSequence);

        var shouldUseSeedId =
            blockDto.Id.HasValue &&
            !options.IgnoreTopLevelIds;

        var block = shouldUseSeedId
            ? WorkoutBlock.NewBuiltInWithId(
                id: WorkoutBlockId.FromGuid(blockDto.Id!.Value),
                name: blockDto.Name,
                blockType: blockDto.BlockType,
                repeatPrescription: repeatPrescription,
                createdBy: seedUser,
                visibilityScope: blockDto.VisibilityScope)
            : WorkoutBlock.NewBuiltIn(
                name: blockDto.Name,
                blockType: blockDto.BlockType,
                repeatPrescription: repeatPrescription,
                createdBy: seedUser,
                visibilityScope: blockDto.VisibilityScope);

        foreach (var segmentDto in blockDto.Segments.OrderBy(segment => segment.Sequence))
        {
            var segment = ToSegment(
                options,
                segmentDto,
                block.Id,
                catalog,
                workoutName,
                blockDto.Name);

            block.AddSegment(segment);
        }

        return block;
    }

    private static WorkoutBlockSegment ToSegment(
    SeedExecutionOptions options,
    WorkoutBlockSegmentSeedJsonDto segmentDto,
    WorkoutBlockId workoutBlockId,
    ExerciseSeedCatalog catalog,
    string workoutName,
    string blockName)
{
    ArgumentNullException.ThrowIfNull(options);
    ArgumentNullException.ThrowIfNull(segmentDto);

    ValidateSegmentDto(
        segmentDto,
        workoutName,
        blockName);

    var shouldUseSeedId =
        segmentDto.Id.HasValue &&
        !options.IgnoreTopLevelIds;

    var segment = shouldUseSeedId
        ? WorkoutBlockSegment.NewWithId(
            id: WorkoutBlockSegmentId.FromGuid(segmentDto.Id!.Value),
            workoutBlockId: workoutBlockId,
            name: segmentDto.Name,
            sequence: segmentDto.Sequence,
            workMode: segmentDto.WorkMode,
            intent: ToWorkIntentPrescription(segmentDto.Intent),
            scoreType: segmentDto.ScoreType,
            timeConstraint: ToTimeConstraint(segmentDto.TimeConstraint),
            intervalPrescription: ToIntervalPrescription(segmentDto.IntervalPrescription),
            estimatedSegmentDuration: ToEstimatedDuration(segmentDto.EstimatedSegmentDuration),
            restAfterStep: ToRestTarget(segmentDto.RestAfterStep),
            restAfterSegment: ToRestTarget(segmentDto.RestAfterSegment))
        : WorkoutBlockSegment.New(
            workoutBlockId: workoutBlockId,
            name: segmentDto.Name,
            sequence: segmentDto.Sequence,
            workMode: segmentDto.WorkMode,
            intent: ToWorkIntentPrescription(segmentDto.Intent),
            scoreType: segmentDto.ScoreType,
            timeConstraint: ToTimeConstraint(segmentDto.TimeConstraint),
            intervalPrescription: ToIntervalPrescription(segmentDto.IntervalPrescription),
            estimatedSegmentDuration: ToEstimatedDuration(segmentDto.EstimatedSegmentDuration),
            restAfterStep: ToRestTarget(segmentDto.RestAfterStep),
            restAfterSegment: ToRestTarget(segmentDto.RestAfterSegment));

    foreach (var stepDto in segmentDto.Steps.OrderBy(step => step.Sequence))
    {
        var step = ToStep(
            options,
            stepDto,
            segment.Id,
            catalog,
            workoutName,
            blockName,
            segmentDto.Name);

        segment.AddStep(step);
    }

    return segment;
}

    private static void ValidateSegmentDto(
        WorkoutBlockSegmentSeedJsonDto segmentDto,
        string workoutName,
        string blockName)
    {
        if (string.IsNullOrWhiteSpace(segmentDto.Name))
        {
            throw new InvalidOperationException(
                $"Workout seed '{workoutName}' block '{blockName}' has a segment with an empty name.");
        }

        if (segmentDto.Id == Guid.Empty)
        {
            throw new InvalidOperationException(
                $"Workout seed '{workoutName}' block '{blockName}' segment '{segmentDto.Name}' has an empty id. Omit the Id property or provide a valid id.");
        }

        if (segmentDto.Steps.Count == 0)
        {
            throw new InvalidOperationException(
                $"Workout seed '{workoutName}' block '{blockName}' segment '{segmentDto.Name}' must provide at least one step.");
        }

        ValidateStepSequences(
            segmentDto.Steps,
            workoutName,
            blockName,
            segmentDto.Name);
    }

    private static void ValidateStepSequences(
        IReadOnlyList<WorkoutBlockSegmentStepSeedJsonDto> steps,
        string workoutName,
        string blockName,
        string segmentName)
    {
        foreach (var stepDto in steps)
        {
            if (stepDto.Sequence < 1)
            {
                throw new InvalidOperationException(
                    $"Workout seed '{workoutName}' block '{blockName}' segment '{segmentName}' has step with invalid sequence '{stepDto.Sequence}'. Sequence must be greater than zero.");
            }
        }

        var duplicateSequences = steps
            .GroupBy(step => step.Sequence)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateSequences.Count > 0)
        {
            throw new InvalidOperationException(
                $"Workout seed '{workoutName}' block '{blockName}' segment '{segmentName}' has duplicate step sequence(s): {string.Join(", ", duplicateSequences)}.");
        }
    }

    private static WorkoutBlockSegmentStep ToStep(
        SeedExecutionOptions options,
        WorkoutBlockSegmentStepSeedJsonDto stepDto,
        WorkoutBlockSegmentId segmentId,
        ExerciseSeedCatalog catalog,
        string workoutName,
        string blockName,
        string segmentName)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(stepDto);

        if (stepDto.Id == Guid.Empty)
        {
            throw new InvalidOperationException(
                $"Workout seed '{workoutName}' block '{blockName}' segment '{segmentName}' step '{stepDto.Sequence}' has an empty id. Omit the Id property or provide a valid id.");
        }

        return stepDto.StepKind switch
        {
            WorkoutStepKind.Exercise => ToExerciseStep(
                options,
                stepDto,
                segmentId,
                catalog,
                workoutName,
                blockName,
                segmentName),

            WorkoutStepKind.Rest => ToRestStep(
                stepDto,
                segmentId,
                workoutName,
                blockName,
                segmentName),

            WorkoutStepKind.Instruction => ToInstructionStep(
                stepDto,
                segmentId,
                workoutName,
                blockName,
                segmentName),

            _ => throw new InvalidOperationException(
                $"Workout seed '{workoutName}' block '{blockName}' segment '{segmentName}' step '{stepDto.Sequence}' has unsupported step kind '{stepDto.StepKind}'.")
        };
    }

    private static WorkoutBlockSegmentStep ToExerciseStep(
        SeedExecutionOptions options,
        WorkoutBlockSegmentStepSeedJsonDto stepDto,
        WorkoutBlockSegmentId segmentId,
        ExerciseSeedCatalog catalog,
        string workoutName,
        string blockName,
        string segmentName)
    {
        if (stepDto.Exercise is null)
        {
            throw new InvalidOperationException(
                $"Workout seed '{workoutName}' block '{blockName}' segment '{segmentName}' exercise step '{stepDto.Sequence}' must provide Exercise.");
        }

        if (stepDto.Prescription is null)
        {
            throw new InvalidOperationException(
                $"Workout seed '{workoutName}' block '{blockName}' segment '{segmentName}' exercise step '{stepDto.Sequence}' must provide Prescription.");
        }

        var exercise = ResolveExercise(
            options,
            stepDto.Exercise,
            catalog,
            workoutName,
            blockName,
            segmentName,
            stepDto.Sequence);

        return WorkoutBlockSegmentStep.NewExerciseStep(
            segmentId: segmentId,
            exerciseId: exercise.Id,
            sequence: stepDto.Sequence,
            prescription: ToWorkoutStepPrescription(
                options,
                stepDto.Prescription),
            notes: stepDto.Notes);
    }
    

    private static WorkoutBlockSegmentStep ToRestStep(
        WorkoutBlockSegmentStepSeedJsonDto stepDto,
        WorkoutBlockSegmentId segmentId,
        string workoutName,
        string blockName,
        string segmentName)
    {
        if (stepDto.Rest is null)
        {
            throw new InvalidOperationException(
                $"Workout seed '{workoutName}' block '{blockName}' segment '{segmentName}' rest step '{stepDto.Sequence}' must provide Rest.");
        }

        return WorkoutBlockSegmentStep.NewRestStep(
            segmentId: segmentId,
            rest: ToRestTarget(stepDto.Rest),
            sequence: stepDto.Sequence,
            notes: stepDto.Notes);
    }

    private static WorkoutBlockSegmentStep ToInstructionStep(
        WorkoutBlockSegmentStepSeedJsonDto stepDto,
        WorkoutBlockSegmentId segmentId,
        string workoutName,
        string blockName,
        string segmentName)
    {
        if (string.IsNullOrWhiteSpace(stepDto.Notes))
        {
            throw new InvalidOperationException(
                $"Workout seed '{workoutName}' block '{blockName}' segment '{segmentName}' instruction step '{stepDto.Sequence}' must provide Notes.");
        }

        return WorkoutBlockSegmentStep.NewInstructionStep(
            segmentId: segmentId,
            notes: stepDto.Notes,
            sequence: stepDto.Sequence);
    }
    
    private static Exercise ResolveExercise(
        SeedExecutionOptions options,
        SeedEntityReferenceJsonDto reference,
        ExerciseSeedCatalog catalog,
        string workoutName,
        string blockName,
        string segmentName,
        int stepSequence)
    {
        if (reference.Id.HasValue && !options.IgnoreReferenceIds)
        {
            return catalog.GetRequiredById(
                ExerciseId.FromGuid(reference.Id.Value));
        }

        if (!string.IsNullOrWhiteSpace(reference.NameKey))
        {
            return catalog.GetRequiredByNameKey(reference.NameKey);
        }

        if (!string.IsNullOrWhiteSpace(reference.Name))
        {
            return catalog.GetRequiredByName(reference.Name);
        }

        throw new InvalidOperationException(
            $"Workout seed '{workoutName}' block '{blockName}' segment '{segmentName}' step '{stepSequence}' exercise reference must provide Id, NameKey, or Name.");
    }

    private static BlockRepeatPrescription ToBlockRepeatPrescription(
        BlockRepeatPrescriptionSeedJsonDto dto,
        string workoutName,
        int blockSequence)
    {
        if (dto.RepeatCount < 1)
        {
            throw new InvalidOperationException(
                $"Workout seed '{workoutName}' block '{blockSequence}' must provide RepeatCount greater than zero.");
        }

        return BlockRepeatPrescription.Repeat(
            repeatCount: dto.RepeatCount,
            prepareTime: dto.PrepareTime,
            restBetweenRepeats: ToRestTarget(dto.RestBetweenRepeats),
            restAfterBlock: ToRestTarget(dto.RestAfterBlock),
            estimatedRepeatDuration: ToEstimatedDuration(dto.EstimatedRepeatDuration));
    }

    private static WorkoutStepPrescription ToWorkoutStepPrescription(
        SeedExecutionOptions options,
        WorkoutStepPrescriptionSeedJsonDto dto)
    {
        var prescription = WorkoutStepPrescription.New(
            workTarget: ToWorkTarget(dto.WorkTarget),
            loadTarget: ToLoadTarget(options, dto.LoadTarget),
            restAfterStep: ToRestTarget(dto.RestAfterStep),
            timeConstraint: ToTimeConstraint(dto.TimeConstraint),
            estimatedDuration: ToEstimatedDuration(dto.EstimatedStepDuration),
            intentOverride: ToNullableWorkIntentPrescription(dto.IntentOverride),
            partition: ToWorkPartitionPrescription(options, dto.Partition),
            notes: dto.Notes);

        return dto.SideExecution.HasValue
            ? prescription.WithLateralityExecution(dto.SideExecution.Value)
            : prescription;
    }

    private static WorkTarget? ToWorkTarget(
        WorkTargetSeedJsonDto? dto)
    {
        if (dto is null)
        {
            return null;
        }

        if (dto.Value <= 0)
        {
            throw new InvalidOperationException(
                $"Work target value must be greater than zero.");
        }

        return WorkTarget.New(
            dto.Value,
            dto.TargetType,
            dto.Scope);
    }

    private static LoadTarget? ToLoadTarget(
        SeedExecutionOptions options,
        LoadTargetSeedJsonDto? dto)
    {
        if (dto is null)
        {
            return null;
        }

        return dto.Type switch
        {
            LoadTargetType.None => LoadTarget.None(),

            LoadTargetType.BodyWeight => LoadTarget.BodyWeight(),

            LoadTargetType.RepMax when dto.Unit == LoadUnit.RM => LoadTarget.RepMax(
                RequireRepMaxReps(dto),
                ToLoadReference(options, dto.LoadReference)),

            LoadTargetType.RepMax when dto.Unit == LoadUnit.Percent => LoadTarget.PercentageRepMax(
                RequireLoadValue(dto),
                ToLoadReference(options, dto.LoadReference),
                RequireReferenceRepMax(dto)),

            LoadTargetType.ExternalLoad => LoadTarget.ExternalLoad(
                RequireLoadValue(dto),
                RequireLoadUnit(dto)),

            LoadTargetType.AddedBodyWeightLoad => LoadTarget.AddedBodyWeightLoad(
                RequireLoadValue(dto),
                RequireLoadUnit(dto)),

            LoadTargetType.AssistedBodyWeight => LoadTarget.AssistedBodyWeight(
                RequireLoadValue(dto),
                RequireLoadUnit(dto)),

            _ => throw new InvalidOperationException(
                $"Unsupported load target type/unit combination '{dto.Type}' / '{dto.Unit}'.")
        };
    }
    
    private static int RequireRepMaxReps(
        LoadTargetSeedJsonDto dto)
    {
        var value = RequireLoadValue(dto);

        if (value % 1 != 0)
        {
            throw new InvalidOperationException(
                $"Load target '{dto.Type}' with unit '{dto.Unit}' must provide a whole-number Value.");
        }

        return (int)value;
    }

    private static decimal RequireLoadValue(
        LoadTargetSeedJsonDto dto)
    {
        if (!dto.Value.HasValue)
        {
            throw new InvalidOperationException(
                $"Load target '{dto.Type}' must provide Value.");
        }

        return dto.Value.Value;
    }

    private static LoadUnit RequireLoadUnit(
        LoadTargetSeedJsonDto dto)
    {
        if (!dto.Unit.HasValue)
        {
            throw new InvalidOperationException(
                $"Load target '{dto.Type}' must provide Unit.");
        }

        return dto.Unit.Value;
    }

    private static int RequireReferenceRepMax(
        LoadTargetSeedJsonDto dto)
    {
        if (!dto.ReferenceRepMax.HasValue)
        {
            throw new InvalidOperationException(
                $"Load target '{dto.Type}' must provide ReferenceRepMax.");
        }

        return dto.ReferenceRepMax.Value;
    }

    private static LoadReference? ToLoadReference(
        SeedExecutionOptions options,
        LoadReferenceSeedJsonDto? dto)
    {
        if (dto is null)
        {
            return null;
        }

        if (dto.Kind == LoadReferenceKind.Named && !string.IsNullOrWhiteSpace(dto.Name))
        {
            return LoadReference.Named(dto.Name);
        }

        if (options.IgnoreReferenceIds)
        {
            throw new InvalidOperationException(
                $"Load reference kind '{dto.Kind}' uses an id-based reference, but seed execution is ignoring reference ids. " +
                "Use a Named load reference in seed JSON, or extend LoadReferenceSeedJsonDto to support NameKey/Name references.");
        }

        return dto.Kind switch
        {
            LoadReferenceKind.Exercise when dto.ExerciseId.HasValue =>
                LoadReference.Exercise(ExerciseId.FromGuid(dto.ExerciseId.Value)),

            LoadReferenceKind.Movement when dto.MovementId.HasValue =>
                LoadReference.Movement(MovementId.FromGuid(dto.MovementId.Value)),

            _ => throw new InvalidOperationException(
                $"Invalid load reference. Kind '{dto.Kind}' must provide the matching ExerciseId, MovementId, or Name.")
        };
    }

    private static RestTarget? ToRestTarget(
        RestTargetSeedJsonDto? dto)
    {
        if (dto is null)
        {
            return null;
        }

        return dto.Policy switch
        {
            RestPolicy.None => RestTarget.None(),

            RestPolicy.Fixed => RestTarget.Fixed(
                RequireSeconds(dto)),

            RestPolicy.NoMoreThan => RestTarget.NoMoreThan(
                RequireSeconds(dto)),

            RestPolicy.AtLeast => RestTarget.AtLeast(
                RequireSeconds(dto)),

            RestPolicy.Range => RestTarget.Range(
                RequireMinimumSeconds(dto),
                RequireMaximumSeconds(dto)),

            RestPolicy.AsNeeded => RestTarget.AsNeeded(),

            RestPolicy.UntilRecovered => RestTarget.UntilRecovered(),

            _ => throw new InvalidOperationException(
                $"Unsupported rest policy '{dto.Policy}'.")
        };
    }

    private static int RequireSeconds(
        RestTargetSeedJsonDto dto)
    {
        if (!dto.Seconds.HasValue)
        {
            throw new InvalidOperationException(
                $"Rest policy '{dto.Policy}' must provide Seconds.");
        }

        if (dto.Seconds.Value < 0)
        {
            throw new InvalidOperationException(
                $"Rest policy '{dto.Policy}' cannot have negative Seconds.");
        }

        return dto.Seconds.Value;
    }

    private static int RequireMinimumSeconds(
        RestTargetSeedJsonDto dto)
    {
        if (!dto.MinimumSeconds.HasValue)
        {
            throw new InvalidOperationException(
                $"Rest policy '{dto.Policy}' must provide MinimumSeconds.");
        }

        return dto.MinimumSeconds.Value;
    }

    private static int RequireMaximumSeconds(
        RestTargetSeedJsonDto dto)
    {
        if (!dto.MaximumSeconds.HasValue)
        {
            throw new InvalidOperationException(
                $"Rest policy '{dto.Policy}' must provide MaximumSeconds.");
        }

        return dto.MaximumSeconds.Value;
    }

    private static TimeConstraint? ToTimeConstraint(
        TimeConstraintSeedJsonDto? dto)
    {
        if (dto is null)
        {
            return null;
        }

        return dto.Kind switch
        {
            TimeConstraintKind.Target => TimeConstraint.Target(
                RequireDuration(dto)),

            TimeConstraintKind.Cap => TimeConstraint.Cap(
                RequireDuration(dto)),

            TimeConstraintKind.Minimum => TimeConstraint.Minimum(
                RequireDuration(dto)),

            TimeConstraintKind.Window => TimeConstraint.Window(
                RequireDuration(dto)),

            TimeConstraintKind.RemainingSegmentTime => TimeConstraint.RemainingSegmentTime(),

            _ => throw new InvalidOperationException(
                $"Unsupported time constraint kind '{dto.Kind}'.")
        };
    }

    private static TimeSpan RequireDuration(
        TimeConstraintSeedJsonDto dto)
    {
        if (!dto.Duration.HasValue)
        {
            throw new InvalidOperationException(
                $"Time constraint '{dto.Kind}' must provide Duration.");
        }

        return dto.Duration.Value;
    }

    private static IntervalPrescription? ToIntervalPrescription(
        IntervalPrescriptionSeedJsonDto? dto)
    {
        if (dto is null)
        {
            return null;
        }

        return dto.Scope switch
        {
            IntervalScope.PerStep => IntervalPrescription.PerStep(
                dto.Duration,
                dto.StartsOnClock),

            IntervalScope.PerSegment => IntervalPrescription.PerSegment(
                dto.Duration,
                dto.StartsOnClock),

            IntervalScope.PerBlockRepeat => IntervalPrescription.PerBlockRepeat(
                dto.Duration,
                dto.StartsOnClock),

            _ => throw new InvalidOperationException(
                $"Unsupported interval scope '{dto.Scope}'.")
        };
    }

    private static EstimatedDuration? ToEstimatedDuration(
        EstimatedDurationSeedJsonDto? dto)
    {
        if (dto is null)
        {
            return null;
        }

        return EstimatedDuration.New(
            expected: dto.Expected,
            minimum: dto.Minimum,
            maximum: dto.Maximum);
    }

    private static WorkIntentPrescription ToWorkIntentPrescription(
        WorkIntentPrescriptionSeedJsonDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        return WorkIntentPrescription.New(
            dto.WorkIntent,
            ToTargetIntensity(dto.TargetIntensity));
    }

    private static WorkIntentPrescription? ToNullableWorkIntentPrescription(
        WorkIntentPrescriptionSeedJsonDto? dto)
    {
        return dto is null
            ? null
            : ToWorkIntentPrescription(dto);
    }

    private static WorkPartitionPrescription? ToWorkPartitionPrescription(
        SeedExecutionOptions options,
        WorkPartitionPrescriptionSeedJsonDto? dto)
    {
        if (dto is null)
        {
            return null;
        }

        return dto.Strategy switch
        {
            WorkPartitionStrategy.Repeated => WorkPartitionPrescription.Repeated(
                repeatCount: RequireRepeatCount(dto),
                restBetweenRepeats: ToRestTarget(dto.RestBetweenRepeats)),

            WorkPartitionStrategy.VariableRepeats => WorkPartitionPrescription.VariableRepeats(
                repeatDetails: dto.RepeatDetails
                    .OrderBy(repeat => repeat.Sequence)
                    .Select(repeat => ToWorkRepeatPrescription(options, repeat))
                    .ToList(),
                restBetweenRepeats: ToRestTarget(dto.RestBetweenRepeats)),

            WorkPartitionStrategy.SplitAnyhow => WorkPartitionPrescription.SplitAnyhow(),

            WorkPartitionStrategy.Unbroken => WorkPartitionPrescription.Unbroken(),

            _ => throw new InvalidOperationException(
                $"Unsupported work partition strategy '{dto.Strategy}'.")
        };
    }

    private static int RequireRepeatCount(
        WorkPartitionPrescriptionSeedJsonDto dto)
    {
        if (!dto.RepeatCount.HasValue || dto.RepeatCount.Value < 1)
        {
            throw new InvalidOperationException(
                $"Work partition '{dto.Strategy}' must provide RepeatCount greater than zero.");
        }

        return dto.RepeatCount.Value;
    }

    private static WorkRepeatPrescription ToWorkRepeatPrescription(
        SeedExecutionOptions options,
        WorkRepeatPrescriptionSeedJsonDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        if (dto.Sequence < 1)
        {
            throw new InvalidOperationException(
                $"Work repeat prescription sequence must be greater than zero.");
        }

        return WorkRepeatPrescription.New(
            sequence: dto.Sequence,
            workTarget: ToWorkTarget(dto.WorkTarget),
            loadTarget: ToLoadTarget(options, dto.LoadTarget),
            targetIntensity: ToTargetIntensity(dto.TargetIntensity),
            restAfterRepeat: ToRestTarget(dto.RestAfterRepeat),
            notes: dto.Notes);
    }

    private static TargetIntensity? ToTargetIntensity(
        TargetIntensitySeedJsonDto? dto)
    {
        if (dto is null)
        {
            return null;
        }

        return dto.Type switch
        {
            IntensityMeasureType.Rpe when dto.Value.HasValue =>
                TargetIntensity.Rpe(dto.Value.Value),

            IntensityMeasureType.HeartRate when dto.Value.HasValue =>
                TargetIntensity.HeartRate((int)dto.Value.Value),

            IntensityMeasureType.PercentMaxHeartRate when dto.Value.HasValue =>
                TargetIntensity.PercentMaxHeartRate(dto.Value.Value),

            IntensityMeasureType.RepsInTheTank when dto.Value.HasValue =>
                TargetIntensity.RepsInTheTank((int)dto.Value.Value),

            IntensityMeasureType.Watts when dto.Value.HasValue =>
                TargetIntensity.Watts(dto.Value.Value),

            IntensityMeasureType.Zone when dto.Value.HasValue =>
                TargetIntensity.Zone((int)dto.Value.Value),

            IntensityMeasureType.Rpe when dto.Range is not null =>
                TargetIntensity.RpeRange(dto.Range.MinValue, dto.Range.MaxValue),

            IntensityMeasureType.HeartRate when dto.Range is not null =>
                TargetIntensity.HeartRateRange(
                    (int)dto.Range.MinValue,
                    (int)dto.Range.MaxValue),

            IntensityMeasureType.PercentMaxHeartRate when dto.Range is not null =>
                TargetIntensity.PercentMaxHeartRateRange(
                    dto.Range.MinValue,
                    dto.Range.MaxValue),

            IntensityMeasureType.Watts when dto.Range is not null =>
                TargetIntensity.WattsRange(dto.Range.MinValue, dto.Range.MaxValue),

            IntensityMeasureType.Pace when dto.PaceTarget is not null =>
                TargetIntensity.Pace(ToPaceTarget(dto.PaceTarget)),

            _ => throw new InvalidOperationException(
                $"Target intensity '{dto.Type}' must provide a valid Value, Range, or PaceTarget.")
        };
    }

    private static PaceTarget ToPaceTarget(
        PaceTargetSeedJsonDto dto)
    {
        return dto.Unit switch
        {
            PaceUnit.PerKilometer => PaceTarget.PerKilometer(dto.Duration),

            PaceUnit.PerMile => PaceTarget.PerMile(dto.Duration),

            PaceUnit.Per500Meters => PaceTarget.Per500Meters(dto.Duration),

            _ => throw new InvalidOperationException(
                $"Unsupported pace unit '{dto.Unit}'.")
        };
    }
}