using PRLab.API.Dtos.GetDto;
using PRLab.Infrastructure;

namespace PRLab.API.Dtos.UpdateDto.Outcome;

public sealed record MovementCategoryUpdateOutcome(
    UpdateOutcome Outcome,
    UpdateOutcome DescriptorOutcome,
    DescriptorUpdateOutcome? Descriptor,
    MovementCategoryGetDTO? UpdatedState,
    IMessagesContainer? Message = null);
