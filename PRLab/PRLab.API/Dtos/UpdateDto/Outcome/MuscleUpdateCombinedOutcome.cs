using PRLab.Infrastructure;

namespace PRLab.API.Dtos.UpdateDto.Outcome;

public sealed record MuscleUpdateCombinedOutcome(
    MuscleUpdateOutcome? Muscle,
    DescriptorUpdateOutcome? Descriptor,
    IMessagesContainer? Message = null);




