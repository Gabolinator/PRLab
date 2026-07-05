using PRLab.Domain.Model.Join;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Application.Interface.DB.Repositories.Entity;

public interface IWorkoutBlockRepository
{
    Task<WorkoutBlockAssignment> GetTrackedByIdAsync(WorkoutBlockId workoutBlockId, CancellationToken ct);
}