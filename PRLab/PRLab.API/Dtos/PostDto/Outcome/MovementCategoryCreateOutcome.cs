using PRLab.API.Dtos.GetDto;
using PRLab.Infrastructure;

namespace PRLab.API.Dtos.PostDto.Outcome;

public sealed record MovementCategoryCreateOutcome(
    CreateOutcome Outcome,
    MovementCategoryGetDTO? CreatedMovementCategory,
    IMessagesContainer? Message = null);
