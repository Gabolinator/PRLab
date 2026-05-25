using PRLab.Infrastructure;

namespace PRLab.API.Dtos.UpdateDto.Outcome;

public sealed record EquipmentUpdateCombinedOutcome(
    EquipmentUpdateOutcome? Equipment,
    DescriptorUpdateOutcome? Descriptor,
    IMessagesContainer? Message);



    
