using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Prescription;
using PRLab.Domain.Model.Value.Prescription.Common;
using PRLab.Domain.Model.Value.WorkoutValue;

namespace PRLab.Domain.Utilities;

public static class WorkoutDurationCalculator
{
    public static EstimatedDuration? EstimateBlockDuration(WorkoutBlock block)
    {
        ArgumentNullException.ThrowIfNull(block);

        // later:
        // prepare time
        // segment windows/caps
        // segment estimates
        // step estimates
        // rest after steps
        // rest after segments
        // rest between rounds
        // rest after block
        // num rounds

        return null;
    }
    
    public static EstimatedDuration? EstimateBlockDuration(Workout workout)
    {
        ArgumentNullException.ThrowIfNull(workout);

        // later:
        // prepare time
        // segment windows/caps
        // segment estimates
        // step estimates
        // rest after steps
        // rest after segments
        // rest between rounds
        // rest after block
        // num rounds

        return null;
    }
}