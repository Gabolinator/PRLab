using PRLab.Domain.Model.Value.Enum.Prescription.Load;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Domain.Model.Value.Prescription.Load;

public sealed record LoadReference
{
    public LoadReferenceKind Kind { get; init; }

    public ExerciseId? ExerciseId { get; init; }

    public MovementId? MovementId { get; init; }

    public string? Name { get; init; }

    private LoadReference()
    {
        // EF Core
    }

    private LoadReference(
        LoadReferenceKind kind,
        ExerciseId? exerciseId,
        MovementId? movementId,
        string? name)
    {
        ValidateOrThrow(
            kind,
            exerciseId,
            movementId,
            name);

        Kind = kind;
        ExerciseId = exerciseId;
        MovementId = movementId;
        Name = string.IsNullOrWhiteSpace(name)
            ? null
            : name.Trim();
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

    public static LoadReference Named(string name)
    {
        return new LoadReference(
            LoadReferenceKind.Named,
            null,
            null,
            name);
    }

    private static void ValidateOrThrow(
        LoadReferenceKind kind,
        ExerciseId? exerciseId,
        MovementId? movementId,
        string? name)
    {
        switch (kind)
        {
            case LoadReferenceKind.Exercise:
                if (!exerciseId.HasValue || exerciseId.Value.Value == Guid.Empty)
                {
                    throw new ArgumentException(
                        "Exercise load reference requires a non-empty exercise id.",
                        nameof(exerciseId));
                }

                if (movementId.HasValue || !string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException("Exercise load reference should not define movement id or name.");
                }

                break;

            case LoadReferenceKind.Movement:
                if (!movementId.HasValue || movementId.Value.Value == Guid.Empty)
                {
                    throw new ArgumentException(
                        "Movement load reference requires a non-empty movement id.",
                        nameof(movementId));
                }

                if (exerciseId.HasValue || !string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException("Movement load reference should not define exercise id or name.");
                }

                break;

            case LoadReferenceKind.Named:
                if (string.IsNullOrWhiteSpace(name))
                {
                    throw new ArgumentException(
                        "Named load reference requires a name.",
                        nameof(name));
                }

                if (exerciseId.HasValue || movementId.HasValue)
                {
                    throw new ArgumentException("Named load reference should not define exercise id or movement id.");
                }

                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(kind),
                    kind,
                    "Unsupported load reference kind.");
        }
    }
}