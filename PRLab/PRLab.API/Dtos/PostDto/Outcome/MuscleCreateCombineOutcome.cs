using PRLab.Infrastructure;

namespace PRLab.API.Dtos.PostDto.Outcome;

public sealed record MuscleCreateCombineOutcome(
    MuscleCreateOutcome? Muscle,
    DescriptionCreateOutcome? Descriptor,
    IMessagesContainer? Message = null);
