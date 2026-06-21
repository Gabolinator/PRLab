using PRLab.Application.Models.DB.Seeding.Catalog;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Model.Value.Enum.Prescription.Intensity;
using PRLab.Domain.Model.Value.Enum.Prescription.Load;
using PRLab.Domain.Model.Value.Enum.Prescription.Rest;
using PRLab.Domain.Model.Value.Enum.Prescription.Time;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Enum.Workout;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Prescription.Common;
using PRLab.Domain.Model.Value.Prescription.Intensity;
using PRLab.Domain.Model.Value.Prescription.Load;
using PRLab.Domain.Model.Value.Prescription.Rest;
using PRLab.Domain.Model.Value.Prescription.Time;
using PRLab.Domain.Model.Value.Prescription.Work;
using PRLab.Domain.Model.Value.Prescription.Workout;
using PRLab.Domain.Model.Value.WorkoutValue;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Prescription;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Structure;
using PRLab.Infrastructure.DB.Seeding.FromJson.Relations.Interface;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Relations;

public sealed class WorkoutSeedRelationResolver : IWorkoutSeedRelationResolver
{
    public void ApplyRelations(
        Workout workout,
        WorkoutSeedJsonDto seedDto,
        ExerciseSeedCatalog catalog,
        User seedUser)
    {
        ArgumentNullException.ThrowIfNull(workout);
        ArgumentNullException.ThrowIfNull(seedDto);
        ArgumentNullException.ThrowIfNull(catalog);
        ArgumentNullException.ThrowIfNull(seedUser);

        if (seedDto.Blocks.Count == 0)
        {
            throw new InvalidOperationException(
                $"Workout seed '{seedDto.Name}' must provide at least one block.");
        }

        ValidateBlockSequences(seedDto);

        foreach (var assignmentDto in seedDto.Blocks.OrderBy(block => block.Sequence))
        {
            var block = ToWorkoutBlock(
                assignmentDto.Block,
                seedDto.Name,
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

    private static void ValidateBlockSequences(
        WorkoutSeedJsonDto seedDto)
    {
        foreach (var blockDto in seedDto.Blocks)
        {
            if (blockDto.Sequence < 1)
            {
                throw new InvalidOperationException(
                    $"Workout seed '{seedDto.Name}' has block with invalid sequence '{blockDto.Sequence}'. Sequence must be greater than zero.");
            }
        }

        var duplicateSequences = seedDto.Blocks
            .GroupBy(block => block.Sequence)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateSequences.Count > 0)
        {
            throw new InvalidOperationException(
                $"Workout seed '{seedDto.Name}' has duplicate block sequence(s): {string.Join(", ", duplicateSequences)}.");
        }
    }

    private static WorkoutBlock ToWorkoutBlock(
        WorkoutBlockSeedJsonDto blockDto,
        string workoutName,
        int blockSequence,
        ExerciseSeedCatalog catalog,
        User seedUser)
    {
        ArgumentNullException.ThrowIfNull(blockDto);

        ValidateBlockDto(
            blockDto,
            workoutName,
            blockSequence);

        var repeatPrescription = ToBlockRepeatPrescription(
            blockDto.BlockRepeatPrescription,
            workoutName,
            blockSequence);

        var block = blockDto.Id.HasValue
            ? WorkoutBlock.NewBuiltInWithId(
                id: WorkoutBlockId.FromGuid(blockDto.Id.Value),
                name: blockDto.Name,
                blockType: blockDto.BlockType,
                repeatPrescription: repeatPrescription,
                createdBy: seedUser)
            : WorkoutBlock.NewBuiltIn(
                name: blockDto.Name,
                blockType: blockDto.BlockType,
                repeatPrescription: repeatPrescription,
                createdBy: seedUser);

        foreach (var segmentDto in blockDto.Segments.OrderBy(segment => segment.Sequence))
        {
            var segment = ToSegment(
                segmentDto,
                block.Id,
                catalog,
                workoutName,
                blockDto.Name);

            block.AddSegment(segment);
        }

        return block;
    }

    private static void ValidateBlockDto(
        WorkoutBlockSeedJsonDto blockDto,
        string workoutName,
        int blockSequence)
    {
        if (string.IsNullOrWhiteSpace(blockDto.Name))
        {
            throw new InvalidOperationException(
                $"Workout seed '{workoutName}' block '{blockSequence}' must provide a name.");
        }

        if (blockDto.Id == Guid.Empty)
        {
            throw new InvalidOperationException(
                $"Workout seed '{workoutName}' block '{blockDto.Name}' has an empty id. Omit the Id property or provide a valid id.");
        }

        if (blockDto.Segments.Count == 0)
        {
            throw new InvalidOperationException(
                $"Workout seed '{workoutName}' block '{blockDto.Name}' must provide at least one segment.");
        }

        ValidateSegmentSequences(
            blockDto.Segments,
            workoutName,
            blockDto.Name);
    }

    private static void ValidateSegmentSequences(
        IReadOnlyList<WorkoutBlockSegmentSeedJsonDto> segments,
        string workoutName,
        string blockName)
    {
        foreach (var segmentDto in segments)
        {
            if (segmentDto.Sequence < 1)
            {
                throw new InvalidOperationException(
                    $"Workout seed '{workoutName}' block '{blockName}' has segment with invalid sequence '{segmentDto.Sequence}'. Sequence must be greater than zero.");
            }
        }

        var duplicateSequences = segments
            .GroupBy(segment => segment.Sequence)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateSequences.Count > 0)
        {
            throw new InvalidOperationException(
                $"Workout seed '{workoutName}' block '{blockName}' has duplicate segment sequence(s): {string.Join(", ", duplicateSequences)}.");
        }
    }

    private static WorkoutBlockSegment ToSegment(
        WorkoutBlockSegmentSeedJsonDto segmentDto,
        WorkoutBlockId workoutBlockId,
        ExerciseSeedCatalog catalog,
        string workoutName,
        string blockName)
    {
        ArgumentNullException.ThrowIfNull(segmentDto);

        ValidateSegmentDto(
            segmentDto,
            workoutName,
            blockName);

        var segment = segmentDto.Id.HasValue
            ? WorkoutBlockSegment.NewWithId(
                id: WorkoutBlockSegmentId.FromGuid(segmentDto.Id.Value),
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
        WorkoutBlockSegmentStepSeedJsonDto stepDto,
        WorkoutBlockSegmentId segmentId,
        string workoutName,
        string blockName,
        string segmentName)
    {
        ArgumentNullException.ThrowIfNull(stepDto);

        if (stepDto.Id == Guid.Empty)
        {
            throw new InvalidOperationException(
                $"Workout seed '{workoutName}' block '{blockName}' segment '{segmentName}' step '{stepDto.Sequence}' has an empty id. Omit the Id property or provide a valid id.");
        }

        return stepDto.StepKind switch
        {
            WorkoutStepKind.Exercise => ToExerciseStep(
                stepDto,
                segmentId,
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
        WorkoutBlockSegmentStepSeedJsonDto stepDto,
        WorkoutBlockSegmentId segmentId,
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

        throw new InvalidOperationException(
            "Use ToExerciseStep overload that receives ExerciseSeedCatalog.");
    }
    
        private static WorkoutBlockSegmentStep ToStep(
        WorkoutBlockSegmentStepSeedJsonDto stepDto,
        WorkoutBlockSegmentId segmentId,
        ExerciseSeedCatalog catalog,
        string workoutName,
        string blockName,
        string segmentName)
    {
        ArgumentNullException.ThrowIfNull(stepDto);

        if (stepDto.Id == Guid.Empty)
        {
            throw new InvalidOperationException(
                $"Workout seed '{workoutName}' block '{blockName}' segment '{segmentName}' step '{stepDto.Sequence}' has an empty id. Omit the Id property or provide a valid id.");
        }

        return stepDto.StepKind switch
        {
            WorkoutStepKind.Exercise => ToExerciseStep(
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
            stepDto.Exercise,
            catalog,
            workoutName,
            blockName,
            segmentName,
            stepDto.Sequence);

        return WorkoutBlockSegmentStep.NewExerciseStep(
            segmentId: segmentId,
            exercise: exercise,
            sequence: stepDto.Sequence,
            prescription: ToWorkoutStepPrescription(stepDto.Prescription),
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
        SeedEntityReferenceJsonDto reference,
        ExerciseSeedCatalog catalog,
        string workoutName,
        string blockName,
        string segmentName,
        int stepSequence)
    {
        if (reference.Id.HasValue)
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
        WorkoutStepPrescriptionSeedJsonDto dto)
    {
        var prescription = WorkoutStepPrescription.New(
            workTarget: ToWorkTarget(dto.WorkTarget),
            loadTarget: ToLoadTarget(dto.LoadTarget),
            restAfterStep: ToRestTarget(dto.RestAfterStep),
            timeConstraint: ToTimeConstraint(dto.TimeConstraint),
            estimatedDuration: ToEstimatedDuration(dto.EstimatedStepDuration),
            intentOverride: ToNullableWorkIntentPrescription(dto.IntentOverride),
            partition: ToWorkPartitionPrescription(dto.Partition),
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

            LoadTargetType.RepMax => LoadTarget.RepMax(
                RequireReferenceRepMax(dto),
                ToLoadReference(dto.LoadReference)),

            LoadTargetType.PercentageOfOneRepMax => LoadTarget.PercentageRepMax(
                RequireLoadValue(dto),
                ToLoadReference(dto.LoadReference),
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
                $"Unsupported load target type '{dto.Type}'.")
        };
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
        LoadReferenceSeedJsonDto? dto)
    {
        if (dto is null)
        {
            return null;
        }

        return dto.Kind switch
        {
            LoadReferenceKind.Exercise when dto.ExerciseId.HasValue =>
                LoadReference.Exercise(ExerciseId.FromGuid(dto.ExerciseId.Value)),

            LoadReferenceKind.Movement when dto.MovementId.HasValue =>
                LoadReference.Movement(MovementId.FromGuid(dto.MovementId.Value)),

            LoadReferenceKind.Named when !string.IsNullOrWhiteSpace(dto.Name) =>
                LoadReference.Named(dto.Name),

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
                    .Select(ToWorkRepeatPrescription)
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
            loadTarget: ToLoadTarget(dto.LoadTarget),
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