using System.Text.Json.Serialization;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Model.Value.Enum.Prescription.Load;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Enum.Workout;
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
[JsonDerivedType(typeof(WorkoutPauseSemantic), "pause")]
[JsonDerivedType(typeof(WorkoutSetCadenceSemantic), "setCadence")]
[JsonDerivedType(typeof(WorkoutCalorieRateSemantic), "calorieRate")]
[JsonDerivedType(typeof(WorkoutSelfPacedSemantic), "selfPaced")]
[JsonDerivedType(typeof(WorkoutHoldSemantic), "hold")]
[JsonDerivedType(typeof(WorkoutBlockTypeSemantic), "blockType")]
[JsonDerivedType(typeof(WorkoutIntensitySemantic), "intensity")]
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

public enum WorkoutEquipmentRelation
{
    On
}

public sealed record WorkoutEquipmentSemantic(
    WorkoutSemanticOrigin Origin, string Name, WorkoutEquipmentRelation? Relation = null,
    int? Quantity = null, decimal? HeightCentimeters = null) : WorkoutImportSemanticItem(Origin);

public sealed record WorkoutEquipmentQuantitySemantic(WorkoutSemanticOrigin Origin, int Quantity)
    : WorkoutImportSemanticItem(Origin);

public sealed record WorkoutEquipmentHeightSemantic(WorkoutSemanticOrigin Origin, decimal Height)
    : WorkoutImportSemanticItem(Origin);

public sealed record WorkoutEquipmentUnitSemantic(WorkoutSemanticOrigin Origin, string Unit) : WorkoutImportSemanticItem(Origin);

public sealed record WorkoutAddedLoadSemantic(WorkoutSemanticOrigin Origin, bool AddedLoad) : WorkoutImportSemanticItem(Origin);

public sealed record WorkoutDistanceUnitSemantic(WorkoutSemanticOrigin Origin, string Unit) : WorkoutImportSemanticItem(Origin);

public enum WorkoutPausePosition
{
    Top
}

public sealed record WorkoutPauseSemantic(WorkoutSemanticOrigin Origin, WorkoutPausePosition? Position)
    : WorkoutImportSemanticItem(Origin);

public sealed record WorkoutSetCadenceSemantic(WorkoutSemanticOrigin Origin, int Seconds, WorkoutSemanticOrigin WorkOrigin)
    : WorkoutImportSemanticItem(Origin);

public sealed record WorkoutCalorieRateSemantic(WorkoutSemanticOrigin Origin, decimal CaloriesPerHour)
    : WorkoutImportSemanticItem(Origin);

public sealed record WorkoutSelfPacedSemantic(WorkoutSemanticOrigin Origin) : WorkoutImportSemanticItem(Origin);

public sealed record WorkoutHoldSemantic(WorkoutSemanticOrigin Origin, int SecondsPerRep, WorkoutSemanticOrigin WorkOrigin)
    : WorkoutImportSemanticItem(Origin);

public sealed record WorkoutBlockTypeSemantic(WorkoutSemanticOrigin Origin, WorkoutBlockType Type)
    : WorkoutImportSemanticItem(Origin);

public enum WorkoutTrainingIntensity
{
    Low
}

public sealed record WorkoutIntensitySemantic(WorkoutSemanticOrigin Origin, WorkoutTrainingIntensity Intensity)
    : WorkoutImportSemanticItem(Origin);

public sealed record WorkoutImportExerciseInterpretation(
    WorkoutImportExerciseReference Exercise,
    WorkoutSemanticSource Source,
    IReadOnlyList<WorkoutImportSemanticItem> SemanticItems);
