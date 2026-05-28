using PRLab.API.Dtos.GetDto;
using PRLab.Infrastructure;

namespace PRLab.API.Dtos.UpdateDto.Outcome;

public sealed record DescriptorUpdateOutcome(
    UpdateOutcome Outcome,
    DescriptionGetDTO? UpdatedState,
    IMessagesContainer? Message =null);




