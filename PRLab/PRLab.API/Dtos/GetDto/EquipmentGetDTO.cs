using PRLab.API.Dtos.SummaryDto;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.GetDto;

public record EquipmentGetDTO(
    EquipmentId Id,
    string Name,
    DescriptionGetDTO? Descriptor,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    bool IsDeleted = false,
    DataAuthority Authority = DataAuthority.Bidirectional);


