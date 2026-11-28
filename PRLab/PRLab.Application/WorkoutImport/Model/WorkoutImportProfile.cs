using PRLab.Domain.Model.Value.Enum;
using PRLab.Domain.Model.Value.Enum.Workout;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;

using PRLab.Application.Models.ImportDraft;

namespace PRLab.Application.WorkoutImport.Model;

public sealed class WorkoutImportProfile
{
    public required string Name { get; init; }

    public MeasurementSystem PreferredMeasurementSystem { get; init; }
        = MeasurementSystem.Metric;

    public IReadOnlyList<WorkoutImportSourceProfile> SourceProfiles { get; init; }
        = [];

    public WorkoutImportAliases Aliases { get; init; }
        = new();

    public WorkoutImportParsing Parsing { get; init; }
        = new();

    public WorkoutImportMappings Mappings { get; init; }
        = new();

    public string? InterpretationInstructions { get; init; }

    public TProfile GetRequiredSourceProfile<TProfile>()
        where TProfile : WorkoutImportSourceProfile
    {
        var profile = SourceProfiles
            .OfType<TProfile>()
            .SingleOrDefault();

        return profile
               ?? throw new InvalidOperationException(
                   $"Workout import profile '{Name}' does not contain " +
                   $"a source profile of type '{typeof(TProfile).Name}'.");
    }
}

public sealed class WorkoutImportParsing
{
    public bool UseCompactRepLoadSyntax { get; init; }

    public bool? UseRepLoadSeries { get; init; }

    public bool UseZeroLoadPerSide { get; init; }

    public WorkoutInstructionKind? CompactPairRepLoadInstruction { get; init; }

    public bool UseCompactSetCadence { get; init; }

    public IReadOnlyList<string> Separators { get; init; } = [];

    public IReadOnlyList<string> IgnoredLabels { get; init; } = [];
}

public sealed class WorkoutImportMappings
{
    public IReadOnlyDictionary<string, WorkoutTrainingIntensity> Intensities { get; init; }
        = new Dictionary<string, WorkoutTrainingIntensity>();

    public IReadOnlyDictionary<string, WorkoutBlockType> BlockTypes { get; init; }
        = new Dictionary<string, WorkoutBlockType>();
}

public sealed class WorkoutImportAliases
{
    public IReadOnlyDictionary<string, string> EquipmentOn { get; init; } = new Dictionary<string, string>();

    public IReadOnlyDictionary<string, string> CountedEquipment { get; init; } = new Dictionary<string, string>();

    public IReadOnlyDictionary<string, string> MovementQualityCues { get; init; } = new Dictionary<string, string>();

    public IReadOnlyList<string> UnpositionedPauses { get; init; } = [];

    public IReadOnlyList<string> SelfPaced { get; init; } = [];

    public IReadOnlyDictionary<string, string> Equipment { get; init; }
        = new Dictionary<string, string>();

    public IReadOnlyDictionary<string, WorkoutPausePosition> PausePositions { get; init; }
        = new Dictionary<string, WorkoutPausePosition>();

    public IReadOnlyDictionary<string, WorkTargetType> PrescriptionHints { get; init; }
        = new Dictionary<string, WorkTargetType>();

    public IReadOnlyDictionary<string, IReadOnlyList<string>> Exercises { get; init; }
        = new Dictionary<string, IReadOnlyList<string>>();

    public IReadOnlyDictionary<string, WorkoutImportAttributeRule> Attributes { get; init; }
        = new Dictionary<string, WorkoutImportAttributeRule>();
}

public sealed class WorkoutImportAttributeRule
{
    public required WorkoutImportAttributeMatchMode MatchMode { get; init; }

    public required IReadOnlyList<WorkoutImportAttributeAssignment> Assignments { get; init; }
}

public enum WorkoutImportAttributeMatchMode
{
    LeadingQualifier = 1,
    MatchedAlias = 2,
    TrailingQualifier = 3,
}

public sealed class WorkoutImportAttributeAssignment
{
    public WorkoutImportAttributeType Type { get; init; } = WorkoutImportAttributeType.None;

    public required string Value { get; init; }
}


public enum WorkoutImportAttributeType
{
    Technique,
    MovementQuality,
    Equipment,
    EquipmentQuantity,
    EquipmentHeight,
    EquipmentUnit,
    AddedLoad,
    DistanceUnit,
    WorkType,
    None,
    PausePosition,
    EquipmentRelation,
}
