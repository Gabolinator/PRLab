using PRLab.Infrastructure;

namespace PRLab.API.Dtos.PostDto.Outcome;

public sealed record MovementCategoryCreateCombineOutcome(
    MovementCategoryCreateOutcome? MovementCategory,
    DescriptionCreateOutcome? Descriptor,
    IMessagesContainer? Message = null);
