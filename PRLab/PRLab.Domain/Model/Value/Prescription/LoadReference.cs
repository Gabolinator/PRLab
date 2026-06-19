using PRLab.Domain.Model.Value.Enum.Prescription;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Domain.Model.Value.Prescription;

public sealed record LoadReference
{
    public LoadReferenceKind Kind { get; init; }

    public ExerciseId? ExerciseId { get; init; }

    public MovementId? MovementId { get; init; }

    public string? Label { get; init; }

    private LoadReference()
    {
        // EF Core
    }

    private LoadReference(
        LoadReferenceKind kind,
        ExerciseId? exerciseId,
        MovementId? movementId,
        string? label)
    {
        if (kind == LoadReferenceKind.Exercise && exerciseId is null)
        {
            throw new ArgumentException("Exercise load reference requires an exercise id.", nameof(exerciseId));
        }

        if (kind == LoadReferenceKind.Movement && movementId is null)
        {
            throw new ArgumentException("Movement load reference requires a movement id.", nameof(movementId));
        }

        if (kind == LoadReferenceKind.Named && string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Named load reference requires a label.", nameof(label));
        }

        Kind = kind;
        ExerciseId = exerciseId;
        MovementId = movementId;
        Label = label;
    }

    public static LoadReference Exercise(ExerciseId exerciseId)
    {
        return new LoadReference(
            LoadReferenceKind.Exercise,
            exerciseId,
            null,
            null);
    }

    public static LoadReference Movement(MovementId movementId)
    {
        return new LoadReference(
            LoadReferenceKind.Movement,
            null,
            movementId,
            null);
    }

    public static LoadReference Named(string label)
    {
        return new LoadReference(
            LoadReferenceKind.Named,
            null,
            null,
            label);
    }
}