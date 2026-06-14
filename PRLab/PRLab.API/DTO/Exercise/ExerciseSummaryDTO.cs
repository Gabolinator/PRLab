using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.API.DTO.Exercise;

public sealed record ExerciseSummaryDTO(
    ExerciseId Id,
    string Name);