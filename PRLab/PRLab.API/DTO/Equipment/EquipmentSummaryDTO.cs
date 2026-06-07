using PRLab.Domain.Value.Identifier;

namespace PRLab.API.DTO.Equipment;

public record EquipmentSummaryDTO(
    EquipmentId Id,
    string Name);