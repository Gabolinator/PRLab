using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Application.WorkoutImport.Model;

public sealed record WorkoutImportContext
{
    public required UserId UserId { get; init; }

    public required WorkoutImportProfile Profile { get; init; }

    public IReadOnlyList<WorkoutImportExerciseReference> Exercises { get; init; } = [];
}