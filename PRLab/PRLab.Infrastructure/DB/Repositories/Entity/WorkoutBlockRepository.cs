using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Repositories.Entity;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Infrastructure.DB.Context;

namespace PRLab.Infrastructure.DB.Repositories.Entity;

public class WorkoutBlockRepository(PRLabPgDBContext db) : IWorkoutBlockRepository
{
    public async Task<WorkoutBlockAssignment> GetTrackedByIdAsync(WorkoutBlockId workoutBlockId, CancellationToken ct)
    {
        return await db.WorkoutBlockAssignments.FirstOrDefaultAsync(block => block.WorkoutBlockId == workoutBlockId,
            ct) ?? throw new InvalidOperationException();
    }
}