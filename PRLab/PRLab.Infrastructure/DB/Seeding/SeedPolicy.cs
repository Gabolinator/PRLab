using PRLab.Domain;

namespace PRLab.Infrastructure.DB.Seeding;

public static class SeedPolicy
{
    public static int GetSeedOrder(DomainEnum.EntityType type)
    {
        return type switch
        {
            DomainEnum.EntityType.User => 1,
            DomainEnum.EntityType.Description => 2,

            DomainEnum.EntityType.Equipment => 10,
            
            DomainEnum.EntityType.Muscle => 21,
            DomainEnum.EntityType.MuscleAntagonist => 22,
            
            DomainEnum.EntityType.MovementCategory => 30,
            DomainEnum.EntityType.Movement => 31,

            DomainEnum.EntityType.WorkloadProfile => 40,

            DomainEnum.EntityType.Exercise => 50,
            
            DomainEnum.EntityType.Workout => 61,

            DomainEnum.EntityType.Program => 70,
            
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };
    }
}