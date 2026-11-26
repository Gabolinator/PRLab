using PRLab.Domain.Model.Value.Identifier;

using PRLab.Domain.Model.Value.Enum.Prescription.Work;

namespace PRLab.Application.WorkoutImport.Model;

public sealed record WorkoutImportExerciseReference
{
    public required ExerciseId Id { get; init; }

    public required string Name { get; init; }

    public required string NameKey { get; init; }

    public IReadOnlyList<string> Aliases { get; init; } = [];

    public WorkTargetType? DefaultWorkTargetType { get; init; }

    public IReadOnlyCollection<WorkTargetType>? AllowedWorkTargetTypes { get; init; }

    public bool SupportsExternalLoad { get; init; }
}
