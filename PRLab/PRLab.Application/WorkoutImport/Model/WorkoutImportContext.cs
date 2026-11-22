using PRLab.Domain.Model.Value.Enum.Workout;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Application.WorkoutImport.Model;

public sealed record WorkoutImportContext
{
    public required UserId UserId { get; init; }

    public required WorkoutImportProfile Profile { get; init; }

    public IReadOnlyList<WorkoutImportExerciseReference> Exercises { get; init; } = [];
}

public sealed class WorkoutImportProfile
{
    public required string Name { get; init; }

    public IReadOnlyDictionary<string, string> ExerciseAliases { get; init; }
        = new Dictionary<string, string>();

    public IReadOnlyDictionary<string, WorkoutBlockType> BlockMappings { get; init; }
        = new Dictionary<string, WorkoutBlockType>();

    public IReadOnlyList<string> IgnoredLabels { get; init; } = [];

    public string? InterpretationInstructions { get; init; }
}