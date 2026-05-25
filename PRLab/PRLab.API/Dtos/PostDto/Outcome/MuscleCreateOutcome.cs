using PRLab.API.Dtos.GetDto;
using PRLab.Infrastructure;

namespace PRLab.API.Dtos.PostDto.Outcome;

public sealed record MuscleCreateOutcome(
    CreateOutcome Outcome,
    MuscleGetDTO? CreatedMuscle,
    IMessagesContainer? Message = null);
