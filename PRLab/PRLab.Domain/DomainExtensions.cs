using PRLab.Domain.Model.Value.Enum.System;

namespace PRLab.Domain;

public static class DomainExtensions
{
    public static bool IsBaseType(this EntityType type) =>
        type switch
        {
            EntityType.User
                or EntityType.Description 
                or EntityType.Equipment 
                or EntityType.MovementCategory 
                or  EntityType.Muscle
                or  EntityType.Movement
                or EntityType.WorkloadProfile
                or  EntityType.Exercise
                or EntityType.Workout 
                or EntityType.Program => true,
            _ => false,
        };


    public static IReadOnlyList<EntityType> GetDependenciesType(this EntityType type) =>
        type switch
        {
               EntityType.Muscle => [EntityType. MuscleAntagonist],
            _ => [],
        };
    
}