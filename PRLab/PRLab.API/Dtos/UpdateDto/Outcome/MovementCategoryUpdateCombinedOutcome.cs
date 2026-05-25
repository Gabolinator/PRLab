using PRLab.Infrastructure;

namespace PRLab.API.Dtos.UpdateDto.Outcome;

public sealed record MovementCategoryUpdateCombinedOutcome(
    MovementCategoryUpdateOutcome? MovementCategory,
    DescriptorUpdateOutcome? Descriptor,
    IMessagesContainer? Message = null)
{
 
}
