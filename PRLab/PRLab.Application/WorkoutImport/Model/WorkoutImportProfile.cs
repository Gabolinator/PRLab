using PRLab.Domain.Model.Value.Enum.Workout;

namespace PRLab.Application.WorkoutImport.Model;

public sealed class WorkoutImportProfile
{
    public required string Name { get; init; }

    public int? DateColumn { get; init; } = null;

    public IReadOnlyDictionary<string, string> ExerciseAliases { get; init; }
        = new Dictionary<string, string>();

    public IReadOnlyDictionary<string, WorkoutBlockType> BlockMappings { get; init; }
        = new Dictionary<string, WorkoutBlockType>();

    public IReadOnlyList<string> IgnoredLabels { get; init; } = [];

    public string? InterpretationInstructions { get; init; }
}