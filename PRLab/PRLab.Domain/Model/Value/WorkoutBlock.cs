using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Domain.Model.Value;

public class WorkoutBlock
{
    // i think id like the block to be resuable between workout.
    
    public WorkoutBlockId Id { get; init; }

    public WorkoutId WorkoutId { get; private set; }

    public List<WorkoutBlockItem> BlocksItems { get; private set; }
    public int Sequence { get; private set; }
    
    // we need a time prescription - timed capped, as fast as possible , no time limit
    // we need round prescription - fixed number of rounds, emom, for time, 
    // we need rest / pause before start of block
    // we need rest target - rest after a round
    // we need rest after block 
}

public class WorkoutBlockItem
{
    public List<Exercise> Exercises { get; private set; }

    public int Sequence { get; private set; }
    
    // maybe also time prescription for rest and pause 
    // and all from workout block too ? 
}