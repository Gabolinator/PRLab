using System.Text.Json.Serialization;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Model.Value.Enum.Prescription.Load;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Application.Models.ImportDraft;

public sealed record WorkoutSemanticSource(string Text, string? Reference, int Offset, int? ColumnIndex = null)
{
    public int Length => Text.Length;
}

[JsonDerivedType(typeof(WorkoutExerciseSemanticTarget), "exercise")]
[JsonDerivedType(typeof(WorkoutScopeSemanticTarget), "scope")]
public abstract record WorkoutSemanticTarget;

public sealed record WorkoutExerciseSemanticTarget(ExerciseId ExerciseId, string? SourceReference, int SourceOffset)
    : WorkoutSemanticTarget;

public sealed record WorkoutScopeSemanticTarget(WorkoutSemanticScope Scope) : WorkoutSemanticTarget;

public enum WorkoutSemanticScope
{
    Row,
    Workout,
    Block,
    Segment,
    Equipment
}

public sealed record WorkoutSemanticOrigin(
    WorkoutSemanticSource Source, WorkoutSemanticTarget Target, decimal Confidence = 1, int? SeriesIndex = null);

[JsonDerivedType(typeof(WorkoutWorkSemantic), "work")]
[JsonDerivedType(typeof(WorkoutLoadSemantic), "load")]
[JsonDerivedType(typeof(WorkoutRestSemantic), "rest")]
[JsonDerivedType(typeof(WorkoutTimeCapSemantic), "timeCap")]
[JsonDerivedType(typeof(WorkoutWorkWindowSemantic), "workWindow")]
[JsonDerivedType(typeof(WorkoutInstructionSemantic), "instruction")]
[JsonDerivedType(typeof(WorkoutTechniqueSemantic), "technique")]
[JsonDerivedType(typeof(WorkoutMovementQualitySemantic), "movementQuality")]
[JsonDerivedType(typeof(WorkoutEquipmentSemantic), "equipment")]
[JsonDerivedType(typeof(WorkoutEquipmentQuantitySemantic), "equipmentQuantity")]
[JsonDerivedType(typeof(WorkoutEquipmentHeightSemantic), "equipmentHeight")]
[JsonDerivedType(typeof(WorkoutEquipmentUnitSemantic), "equipmentUnit")]
[JsonDerivedType(typeof(WorkoutAddedLoadSemantic), "addedLoad")]
[JsonDerivedType(typeof(WorkoutDistanceUnitSemantic), "distanceUnit")]
public abstract record WorkoutImportSemanticItem(WorkoutSemanticOrigin Origin);

public sealed record WorkoutWorkSemantic(
    WorkoutSemanticOrigin Origin, WorkTargetType TargetType, decimal Value, int? SetCount, WorkTargetScope Scope)
    : WorkoutImportSemanticItem(Origin);

public sealed record WorkoutLoadSemantic(WorkoutSemanticOrigin Origin, decimal Value, LoadUnit Unit)
    : WorkoutImportSemanticItem(Origin);

public sealed record WorkoutRestSemantic(WorkoutSemanticOrigin Origin, int Seconds, bool IsMinimum)
    : WorkoutImportSemanticItem(Origin);

public sealed record WorkoutTimeCapSemantic(WorkoutSemanticOrigin Origin, int Seconds) : WorkoutImportSemanticItem(Origin);

public sealed record WorkoutWorkWindowSemantic(WorkoutSemanticOrigin Origin, WorkMode Mode, int DurationSeconds)
    : WorkoutImportSemanticItem(Origin);

public sealed record WorkoutInstructionSemantic(WorkoutSemanticOrigin Origin, WorkoutInstructionKind Instruction)
    : WorkoutImportSemanticItem(Origin);

public enum WorkoutInstructionKind
{
    SplitAnyhow
}

public sealed record WorkoutTechniqueSemantic(WorkoutSemanticOrigin Origin, string Technique)
    : WorkoutImportSemanticItem(Origin);

public sealed record WorkoutMovementQualitySemantic(WorkoutSemanticOrigin Origin, string Quality)
    : WorkoutImportSemanticItem(Origin);

public sealed record WorkoutEquipmentSemantic(WorkoutSemanticOrigin Origin, string Name) : WorkoutImportSemanticItem(Origin);

public sealed record WorkoutEquipmentQuantitySemantic(WorkoutSemanticOrigin Origin, int Quantity)
    : WorkoutImportSemanticItem(Origin);

public sealed record WorkoutEquipmentHeightSemantic(WorkoutSemanticOrigin Origin, decimal Height)
    : WorkoutImportSemanticItem(Origin);

public sealed record WorkoutEquipmentUnitSemantic(WorkoutSemanticOrigin Origin, string Unit) : WorkoutImportSemanticItem(Origin);

public sealed record WorkoutAddedLoadSemantic(WorkoutSemanticOrigin Origin, bool AddedLoad) : WorkoutImportSemanticItem(Origin);

public sealed record WorkoutDistanceUnitSemantic(WorkoutSemanticOrigin Origin, string Unit) : WorkoutImportSemanticItem(Origin);

public sealed record WorkoutImportExerciseInterpretation(
    WorkoutImportExerciseReference Exercise,
    WorkoutSemanticSource Source,
    IReadOnlyList<WorkoutImportSemanticItem> SemanticItems);
