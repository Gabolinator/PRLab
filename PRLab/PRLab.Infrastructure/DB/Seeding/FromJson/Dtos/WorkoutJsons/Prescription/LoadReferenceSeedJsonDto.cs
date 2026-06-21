using PRLab.Domain.Model.Value.Enum.Prescription.Load;
using PRLab.Domain.Model.Value.Prescription.Load;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.WorkoutJsons.Prescription;

public sealed record LoadReferenceSeedJsonDto
{
    public LoadReferenceKind Kind { get; init; }

    public Guid? ExerciseId { get; init; }

    public Guid? MovementId { get; init; }

    public string? Name { get; init; }

    public static LoadReferenceSeedJsonDto? FromLoadReference(
        LoadReference? loadReference)
    {
        if (loadReference is null)
        {
            return null;
        }

        return new LoadReferenceSeedJsonDto
        {
            Kind = loadReference.Kind,
            ExerciseId = loadReference.ExerciseId?.Value,
            MovementId = loadReference.MovementId?.Value,
            Name = loadReference.Name
        };
    }
}