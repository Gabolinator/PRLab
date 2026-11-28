using System.Globalization;
using PRLab.Application.Models.ImportDraft;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
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
            return
            [
                CreateAttribute(WorkoutImportAttributeType.MovementQuality, "Paused"),
                CreateAttribute(WorkoutImportAttributeType.PausePosition, pause.Position.ToString())
            ];
        }

        var attribute = ResolveAttribute(item);
        return attribute is null ? [] : [attribute];
    }

    private static WorkoutImportAttributeAssignment? ResolveAttribute(WorkoutImportSemanticItem item) =>
        item switch
        {
            WorkoutTechniqueSemantic technique => CreateAttribute(WorkoutImportAttributeType.Technique, technique.Technique),
            WorkoutMovementQualitySemantic quality => CreateAttribute(WorkoutImportAttributeType.MovementQuality, quality.Quality),
            WorkoutEquipmentSemantic equipment => CreateAttribute(WorkoutImportAttributeType.Equipment, equipment.Name),
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
                    WorkoutCalorieRateSemantic or
                    WorkoutRestSemantic or
                    WorkoutTimeCapSemantic or
                    WorkoutInstructionSemantic)
                .OrderBy(item => item.Origin.Source.ColumnIndex)
                .ThenBy(item => item.Origin.Source.Offset)
                .ThenBy(item => item.Origin.SeriesIndex);
        var groups = prescriptionItems.GroupBy(item => item is WorkoutSetCadenceSemantic cadence ? cadence.WorkOrigin : item.Origin);

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
        var calorieRate = group.OfType<WorkoutCalorieRateSemantic>().SingleOrDefault();
        var rest = group.OfType<WorkoutRestSemantic>().SingleOrDefault();
        var timeCap = group.OfType<WorkoutTimeCapSemantic>().SingleOrDefault();
        var instruction = group.OfType<WorkoutInstructionSemantic>().SingleOrDefault();

        var step = new WorkoutStepImportDraft
        {
            SourceText = origin.Source.Text,
            StepKind = rest is not null ? WorkoutStepKind.Rest
                : instruction is not null || timeCap is not null ? WorkoutStepKind.Instruction
                : WorkoutStepKind.Exercise,
            ExerciseName = exercise?.Exercise.Name,
            WorkTargetType = work?.TargetType,
            WorkValue = work?.Value,
            SetCount = work?.SetCount,
            SetCadenceSeconds = cadence?.Seconds,
            CaloriesPerHour = calorieRate?.CaloriesPerHour,
            Scope = work?.Scope ?? WorkTargetScope.Total,
            LoadValue = load?.Value,
            LoadUnit = load?.Unit,
            RestSeconds = rest is { IsMinimum: false } ? rest.Seconds : null,
            MinimumRestSeconds = rest is { IsMinimum: true } ? rest.Seconds : null,
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
