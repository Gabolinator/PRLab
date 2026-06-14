using PRLab.API.DTO.Description;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.API.DTO.Equipment;

public record EquipmentGetDTO(
    EquipmentId Id,
    string Name,
    DescriptionGetDTO? Descriptor);


