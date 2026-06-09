using PRLab.API.DTO.Movement;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.DTO.Exercise.Relation;

public sealed record ExerciseBlockGetDTO(
    ExerciseBlockId Id,
    int Sequence,
    MovementSummaryDTO Movement,
    WorkTargetGetDTO Target,
    LoadTargetGetDTO LoadTarget,
    RestTargetGetDTO RestBetweenReps,
    RestTargetGetDTO TransitionAfterBlock,
    RepExecutionDetailsGetDTO ExecutionDetails);