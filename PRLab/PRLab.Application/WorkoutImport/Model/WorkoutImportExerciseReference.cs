using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Application.WorkoutImport.Model;

public sealed record WorkoutImportExerciseReference
{
    public required ExerciseId Id { get; init; }

    public required string Name { get; init; }

    public required string NameKey { get; init; }

    public IReadOnlyList<string> Aliases { get; init; } = [];
}