using System.Globalization;
using PRLab.Application.Models.ImportDraft;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Model.Value.Enum.Workout;

namespace PRLab.Infrastructure.WorkoutImport.Interpreter;

internal static class WorkoutSemanticResolver
{
    public static WorkoutImportRowDraft ResolveRow(WorkoutImportRowDraft row)
    {
        var exercisesByTarget =
            row.ExerciseInterpretations
                .ToDictionary(exercise => new WorkoutExerciseSemanticTarget(
                    exercise.Exercise.Id,
                    exercise.Source.Reference,
                    exercise.Source.Offset));

        var exerciseParts =
            row.ExerciseParts
                .Select(part =>
                {
                    var exercise = part.SourceOffset is { } offset && part.ExerciseId is { } exerciseId
                        ? exercisesByTarget.GetValueOrDefault(new WorkoutExerciseSemanticTarget(
                            exerciseId,
                            part.SourceReference,
                            offset))
                        : null;

                    return ResolveExercisePart(part, exercise);
                })
                .ToList();

        var exerciseItems = row.ExerciseInterpretations.SelectMany(exercise => exercise.SemanticItems);
        var items = row.RowSemanticItems.Concat(exerciseItems);
        var workWindows =
            row.RowSemanticItems
                .OfType<WorkoutWorkWindowSemantic>()
                .Select(window => new WorkoutWorkWindowDraft(
                    window.Mode,
                    window.DurationSeconds,
                    window.Origin.Source.Text,
                    window.Origin.Source.Reference))
                .ToList();

        return row with
        {
            ExerciseParts = exerciseParts,
            WorkWindows = workWindows,
            Prescriptions = ResolvePrescriptions(items, exercisesByTarget)
        };
    }

    public static IReadOnlyList<WorkoutBlockImportDraft> ResolveBlocks(
        IReadOnlyList<WorkoutImportRowDraft> rows)
    {
        var blockRows = rows.Where(row => row.WorkWindows.Count == 1);

        return blockRows
            .Select(WorkoutBlockImportDraft.CreateFromRow)
            .ToList();
    }

    private static WorkoutImportExercisePart ResolveExercisePart(
        WorkoutImportExercisePart part,
        WorkoutImportExerciseInterpretation? exercise)
    {
        if (exercise is not null)
        {
            var attributes =
                exercise.SemanticItems
                    .SelectMany(ResolveAttributes)
                    .DistinctBy(attribute => (attribute.Type, attribute.Value))
                    .ToList();

            part = part with { Attributes = attributes };
        }

        var possiblyMissing =
            part.ExpectedAttributes
                .Where(attribute => !part.HasAttribute(attribute))
                .ToList();

        return part.WithPossiblyMissingAttributes(possiblyMissing);
    }

    private static IEnumerable<WorkoutImportAttributeAssignment> ResolveAttributes(WorkoutImportSemanticItem item)
    {
        if (item is WorkoutPauseSemantic pause)
        {
            var quality = CreateAttribute(WorkoutImportAttributeType.MovementQuality, "Paused");
            return pause.Position is { } position
                ? [quality, CreateAttribute(WorkoutImportAttributeType.PausePosition, position.ToString())]
                : [quality];
        }

        if (item is WorkoutEquipmentSemantic equipment)
        {
            return ResolveEquipment(equipment);
        }

        var attribute = ResolveAttribute(item);
        return attribute is null ? [] : [attribute];
    }

    private static WorkoutImportAttributeAssignment? ResolveAttribute(WorkoutImportSemanticItem item) =>
        item switch
        {
            WorkoutTechniqueSemantic technique => CreateAttribute(WorkoutImportAttributeType.Technique, technique.Technique),
            WorkoutMovementQualitySemantic quality => CreateAttribute(WorkoutImportAttributeType.MovementQuality, quality.Quality),
            WorkoutEquipmentQuantitySemantic quantity => CreateAttribute(
                WorkoutImportAttributeType.EquipmentQuantity,
                quantity.Quantity.ToString(CultureInfo.InvariantCulture)),
            WorkoutEquipmentHeightSemantic height => CreateAttribute(
                WorkoutImportAttributeType.EquipmentHeight,
                height.Height.ToString(CultureInfo.InvariantCulture)),
            WorkoutEquipmentUnitSemantic unit => CreateAttribute(WorkoutImportAttributeType.EquipmentUnit, unit.Unit),
            WorkoutAddedLoadSemantic load => CreateAttribute(WorkoutImportAttributeType.AddedLoad, load.AddedLoad ? "true" : "false"),
            WorkoutDistanceUnitSemantic distance => CreateAttribute(WorkoutImportAttributeType.DistanceUnit, distance.Unit),
            _ => null
        };

    private static IReadOnlyList<WorkoutImportAttributeAssignment> ResolveEquipment(WorkoutEquipmentSemantic equipment)
    {
        var attributes = new List<WorkoutImportAttributeAssignment>
        {
            CreateAttribute(WorkoutImportAttributeType.Equipment, equipment.Name)
        };
        if (equipment.Relation is { } relation)
        {
            attributes.Add(CreateAttribute(WorkoutImportAttributeType.EquipmentRelation, relation.ToString()));
        }

        if (equipment.Quantity is { } quantity)
        {
            attributes.Add(CreateAttribute(WorkoutImportAttributeType.EquipmentQuantity, quantity.ToString(CultureInfo.InvariantCulture)));
        }

        if (equipment.HeightCentimeters is { } height)
        {
            attributes.Add(CreateAttribute(WorkoutImportAttributeType.EquipmentHeight, height.ToString(CultureInfo.InvariantCulture)));
            attributes.Add(CreateAttribute(WorkoutImportAttributeType.EquipmentUnit, "cm"));
        }

        return attributes;
    }

    private static WorkoutImportAttributeAssignment CreateAttribute(WorkoutImportAttributeType type, string value) =>
        new() { Type = type, Value = value };

    private static IReadOnlyList<WorkoutImportPrescriptionDraft> ResolvePrescriptions(
        IEnumerable<WorkoutImportSemanticItem> items,
        IReadOnlyDictionary<WorkoutExerciseSemanticTarget, WorkoutImportExerciseInterpretation> exercisesByTarget)
    {
        var prescriptionItems =
            items
                .Where(item => item is WorkoutWorkSemantic or
                    WorkoutLoadSemantic or
                    WorkoutSetCadenceSemantic or
                    WorkoutHoldSemantic or
                    WorkoutPauseSemantic or
                    WorkoutSelfPacedSemantic or
                    WorkoutCalorieRateSemantic or
                    WorkoutRestSemantic or
                    WorkoutTimeCapSemantic or
                    WorkoutInstructionSemantic)
                .OrderBy(item => item.Origin.Source.ColumnIndex)
                .ThenBy(item => item.Origin.Source.Offset)
                .ThenBy(item => item.Origin.SeriesIndex);
        var groups = prescriptionItems.GroupBy(item => item switch
        {
            WorkoutSetCadenceSemantic cadence => cadence.WorkOrigin,
            WorkoutHoldSemantic hold => hold.WorkOrigin,
            _ => item.Origin
        });

        return groups
            .Select(group => ResolvePrescription(group, exercisesByTarget))
            .ToList();
    }

    private static WorkoutImportPrescriptionDraft ResolvePrescription(
        IGrouping<WorkoutSemanticOrigin, WorkoutImportSemanticItem> group,
        IReadOnlyDictionary<WorkoutExerciseSemanticTarget, WorkoutImportExerciseInterpretation> exercisesByTarget)
    {
        var origin = group.Key;
        var exerciseTarget = origin.Target as WorkoutExerciseSemanticTarget;
        var exercise = exerciseTarget is null ? null : exercisesByTarget.GetValueOrDefault(exerciseTarget);

        var work = group.OfType<WorkoutWorkSemantic>().SingleOrDefault();
        var load = group.OfType<WorkoutLoadSemantic>().SingleOrDefault();
        var cadence = group.OfType<WorkoutSetCadenceSemantic>().SingleOrDefault();
        var hold = group.OfType<WorkoutHoldSemantic>().SingleOrDefault();
        var pause = group.OfType<WorkoutPauseSemantic>().MaxBy(item => item.Position.HasValue);
        var selfPaced = group.OfType<WorkoutSelfPacedSemantic>().Any();
        var calorieRate = group.OfType<WorkoutCalorieRateSemantic>().SingleOrDefault();
        var rest = group.OfType<WorkoutRestSemantic>().SingleOrDefault();
        var timeCap = group.OfType<WorkoutTimeCapSemantic>().SingleOrDefault();
        var instruction = group.OfType<WorkoutInstructionSemantic>().SingleOrDefault();

        var step = new WorkoutStepImportDraft
        {
            SourceText = origin.Source.Text,
            StepKind = rest is not null ? WorkoutStepKind.Rest
                : instruction is not null || timeCap is not null || selfPaced || (pause is not null && work is null && load is null)
                    ? WorkoutStepKind.Instruction
                : WorkoutStepKind.Exercise,
            ExerciseName = exercise?.Exercise.Name,
            Work = work is null ? null : WorkoutWorkImportDraft.FromSemantic(work),
            Load = load is null ? null : WorkoutLoadImportDraft.FromSemantic(load),
            Rest = rest is null ? null : WorkoutRestImportDraft.FromSemantic(rest),
            Cadence = WorkoutCadenceImportDraft.Create(cadence?.Seconds, selfPaced),
            Execution = WorkoutExecutionImportDraft.Create(
                hold is null ? null : WorkoutHoldImportDraft.FromSemantic(hold),
                pause is null ? null : WorkoutPauseImportDraft.FromSemantic(pause)),
            Rate = calorieRate is null ? null : WorkoutRateImportDraft.FromSemantic(calorieRate),
            Notes = instruction is null ? null : origin.Source.Text,
            Confidence = origin.Confidence
        };

        return new WorkoutImportPrescriptionDraft(origin.Source.Reference, step, timeCap?.Seconds)
        {
            ExerciseId = exerciseTarget?.ExerciseId,
            ExerciseSourceReference = exerciseTarget?.SourceReference,
            ExerciseSourceOffset = exerciseTarget?.SourceOffset
        };
    }
}
