using PRLab.API.Dtos.GetDto;
using PRLab.Infrastructure;

namespace PRLab.API.Dtos.UpdateDto.Outcome;

public sealed record MuscleUpdateOutcome(
    UpdateOutcome Outcome,
    UpdateOutcome DescriptorOutcome,
    DescriptorUpdateOutcome? Descriptor,
    MuscleGetDTO? UpdatedState,
    IMessagesContainer? Message = null);
