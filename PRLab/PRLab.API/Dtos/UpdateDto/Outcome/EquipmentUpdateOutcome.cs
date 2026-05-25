using PRLab.API.Dtos.GetDto;
using PRLab.Infrastructure;

namespace PRLab.API.Dtos.UpdateDto.Outcome;

public sealed record EquipmentUpdateOutcome(
    UpdateOutcome Outcome,
    UpdateOutcome DescriptorOutcome,
    DescriptorUpdateOutcome? Descriptor,
    EquipmentGetDTO? UpdatedState,
    IMessagesContainer? Message = null);


