namespace PRLab.API.Dtos.UpdateDto.Request;

public sealed record MuscleUpdateRequest(
    MuscleUpdateDTO Muscle,
    Guid CorrelationId,
    UpdateRequest UpdateRequest,
    string RequestedBy);
