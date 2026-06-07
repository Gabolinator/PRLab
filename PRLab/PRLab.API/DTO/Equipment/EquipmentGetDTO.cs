using PRLab.API.DTO.Description;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.DTO.Equipment;

public record EquipmentGetDTO(
    EquipmentId Id,
    string Name,
    DescriptionGetDTO? Descriptor);


