using PRLab.Infrastructure;

namespace PRLab.API.Dtos.PostDto.Outcome;

public record EquipmentCreateCombineOutcome(
    EquipmentCreateOutcome? CreatedEquipmentOutcome,
    DescriptionCreateOutcome? CreatedDescriptorOutcome, IMessagesContainer? Message =null);