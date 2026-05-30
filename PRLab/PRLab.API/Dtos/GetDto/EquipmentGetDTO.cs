using PRLab.API.Dtos.SummaryDto;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.GetDto;

public record EquipmentGetDTO(
    EquipmentId Id,
    string Name,
    DescriptionGetDTO? Descriptor);


