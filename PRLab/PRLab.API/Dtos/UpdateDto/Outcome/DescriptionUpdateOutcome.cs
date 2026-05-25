using PRLab.API.Dtos.GetDto;
using PRLab.Infrastructure;

namespace PRLab.API.Dtos.UpdateDto.Outcome;

public sealed record DescriptorUpdateOutcome(
    UpdateOutcome Outcome,
    DescriptorGetDTO? UpdatedState,
    IMessagesContainer? Message =null);




