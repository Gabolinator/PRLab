using PRLab.API.DTO.Workout;
using PRLab.API.DTO.Workout.WorkoutBlockAssignment;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Utilities;

namespace PRLab.API.Mapper.WorkoutMappers;

public static class WorkoutMapper
{
    public static IReadOnlyCollection<WorkoutGetDTO> ToGetDTOs(
        IReadOnlyCollection<Workout> workouts)
    {
        return ToGetDTOs(
            workouts,
            (LocalizationHelper.Language?)null);
    }

    public static IReadOnlyCollection<WorkoutGetDTO> ToGetDTOs(
        IReadOnlyCollection<Workout> workouts,
        LocalizationHelper.Language? language)
    {
        ArgumentNullException.ThrowIfNull(workouts);

        return workouts
            .Select(workout => ToGetDTO(workout, language))
            .ToList();
    }

    public static WorkoutGetDTO ToGetDTO(Workout workout)
    {
        return ToGetDTO(
            workout,
            (LocalizationHelper.Language?)null);
    }

    public static WorkoutGetDTO ToGetDTO(
        Workout workout,
        LocalizationHelper.Language? language)
    {
        ArgumentNullException.ThrowIfNull(workout);

        return new WorkoutGetDTO(
            workout.Id,
            workout.Name,
            DescriptionMapper.ToGetDTO(
                workout.Description,
                language),
            workout.EstimatedDuration,
            workout.Blocks
                .OrderBy(assignment => assignment.Sequence)
                .Select(assignment =>
                    new WorkoutBlockAssignmentGetDTO(
                        assignment.Id,
                        assignment.Sequence,
                        WorkoutBlockMapper.ToGetDTO(
                            assignment.WorkoutBlock,
                            language)))
                .ToList(),
            workout.Visibility.Scope);
    }

    public static Workout ToEntity(
        WorkoutPostDTO payload,
        User user)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(user);

        ValidateBlockSequences(payload.Blocks);

        var description = payload.Description is null
            ? Description.New(null)
            : DescriptionMapper.ToEntity(payload.Description);

        var workout = Workout.NewUserCreated(
            name: payload.Name,
            description: description,
            owner: user,
            visibilityScope: payload.Visibility);

        if (payload.EstimatedDuration is not null)
        {
            workout.ChangeEstimatedDuration(
                payload.EstimatedDuration,
                user);
        }

        foreach (var blockPayload in payload.Blocks
                     .OrderBy(block => block.Sequence))
        {
            var workoutBlock = WorkoutBlockMapper.ToEntity(
                blockPayload.Block,
                user);

            var assignment = WorkoutBlockAssignment.New(
                workoutId: workout.Id,
                workoutBlock: workoutBlock,
                sequence: blockPayload.Sequence);

            workout.AddBlock(
                assignment,
                user,
                blockPayload.Sequence);
        }

        return workout;
    }

    private static void ValidateBlockSequences(
        IReadOnlyCollection<WorkoutBlockAssignmentPostDTO> blocks)
    {
        ArgumentNullException.ThrowIfNull(blocks);

        var orderedSequences = blocks
            .Select(block => block.Sequence)
            .OrderBy(sequence => sequence)
            .ToList();

        for (var index = 0; index < orderedSequences.Count; index++)
        {
            var expectedSequence = index + 1;

            if (orderedSequences[index] != expectedSequence)
            {
                throw new ArgumentException(
                    "Workout block sequences must start at one and be contiguous.",
                    nameof(blocks));
            }
        }
    }
}