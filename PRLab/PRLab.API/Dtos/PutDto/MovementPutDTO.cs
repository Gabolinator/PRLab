using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.PutDto;

public record MovementPutDTO(
    string Name,
    DescriptionId? DescriptionId,
    MovementCategoryId? CategoryId,
    IReadOnlyList<EquipmentId> EquipmentIds,
    IReadOnlyList<MuscleId> PrimaryMuscleIds,
    IReadOnlyList<MuscleId> SecondaryMuscleIds,
    MovementId? VariantOfMovementId);