using PRLab.API.DTO.Movement;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.API.DTO.Exercise.Relation;

public sealed record ExerciseStepGetDTO(
    ExerciseStepsId Id,
    int Sequence,
    MovementSummaryDTO Movement,
    WorkTargetGetDTO Target,
    LoadTargetGetDTO LoadTarget,
    RestTargetGetDTO RestBetweenReps,
    RestTargetGetDTO TransitionAfterBlock,
    RepExecutionDetailsGetDTO ExecutionDetails);