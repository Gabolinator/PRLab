using PRLab.API.Dtos.SummaryDto;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.GetDto;

public record MovementGetDTO(
    MovementId Id,
    string Name,
    DescriptionSummaryDTO? Descriptor,
    MovementCategorySummaryDTO? Category,
    IReadOnlyList<EquipmentSummaryDTO> Equipments,
    IReadOnlyList<SummaryDto.MuscleSummaryDTO> PrimaryMuscles,
    IReadOnlyList<SummaryDto.MuscleSummaryDTO> SecondaryMuscles,
    MovementSummaryDTO? VariantOfMovement,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    long UpdatedSeq,
    bool IsDeleted = false,
    DataAuthority Authority = DataAuthority.Bidirectional);