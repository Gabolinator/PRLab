using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities;

namespace PRLab.Domain.Model.Value.Update;

public sealed class ExerciseUpdate
{
    public string? Name { get; init; }

    public DescriptionUpdate? Description { get; init; }

    public IReadOnlyCollection<ExerciseStepUpdate>? Steps { get; init; }

    public User? UpdatedBy { get; init; }

    public static ExerciseUpdate FromExercise(
        Exercise exercise,
        LocalizationHelper.Language? language,
        User? user)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        return new ExerciseUpdate
        {
            Name = exercise.Name,
            Description = DescriptionUpdate.FromDescription(
                exercise.Description,
                language,
                user),
            Steps = exercise.Steps
                .Select(ExerciseStepUpdate.FromExerciseStep)
                .ToList(),
            UpdatedBy = user
        };
    }
}