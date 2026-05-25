using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.SummaryDto;

public record EquipmentSummaryDTO(
    EquipmentId Id,
    string Name);