using PRLab.API.DTO.Exercise;
using PRLab.API.DTO.Exercise.Relation;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Update;

namespace PRLab.API.Mapper.UpdateMapper;

public static class ExerciseUpdateMapper
{
    public static ExerciseUpdate ToUpdate(
        Exercise exercise,
        ExercisePutDTO payload,
        User? activeUser = null)
    {
        ArgumentNullException.ThrowIfNull(exercise);
        ArgumentNullException.ThrowIfNull(payload);

        return new ExerciseUpdate
        {
            Name = payload.Name,
            Description = payload.Description is null
                ? null
                : DescriptionUpdateMapper.ToUpdate(payload.Description),
            Blocks = payload.Blocks
                .OrderBy(block => block.Sequence)
                .Select(ToBlockUpdate)
                .ToList(),
            UpdatedBy = activeUser
        };
    }

    private static ExerciseBlockUpdate ToBlockUpdate(ExerciseBlockPutDTO payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        return new ExerciseBlockUpdate
        {
            MovementId = payload.MovementId,
            Sequence = payload.Sequence,
            Target = ExerciseMapper.ToWorkTarget(payload.Target),
            LoadTarget = ExerciseMapper.ToLoadTarget(payload.LoadTarget),
            RestBetweenReps = ExerciseMapper.ToRestTarget(payload.RestBetweenReps),
            TransitionAfterBlock = ExerciseMapper.ToRestTarget(payload.TransitionAfterBlock),
            ExecutionDetails = ExerciseMapper.ToRepExecutionDetails(payload.ExecutionDetails)
        };
    }
}