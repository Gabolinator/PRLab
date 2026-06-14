using PRLab.Domain;
using PRLab.Domain.Model.Value.Enum.System;

namespace PRLab.Infrastructure.DB.Seeding;

public static class SeedPolicy
{
    public static int GetSeedOrder(EntityType type)
    {
        return type switch
        {
            EntityType.User => 1,
            EntityType.Description => 2,

            EntityType.Equipment => 10,
            
            EntityType.Muscle => 21,
            EntityType.MuscleAntagonist => 22,
            
            EntityType.MovementCategory => 30,
            EntityType.Movement => 31,

            EntityType.WorkloadProfile => 40,

            EntityType.Exercise => 50,
            
            EntityType.Workout => 61,

            EntityType.Program => 70,
            
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}