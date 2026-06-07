namespace PRLab.Domain;

public static class DomainExtensions
{
    public static bool IsBaseType(this DomainEnum.EntityType type) =>
        type switch
        {
            DomainEnum.EntityType.User
                or DomainEnum.EntityType.Description 
                or DomainEnum.EntityType.Equipment 
                or DomainEnum.EntityType.MovementCategory 
                or  DomainEnum.EntityType.Muscle
                or  DomainEnum.EntityType.Movement
                or DomainEnum.EntityType.WorkloadProfile
                or  DomainEnum.EntityType.Exercise
                or DomainEnum.EntityType.Workout 
                or DomainEnum.EntityType.Program => true,
            _ => false,
        };


    public static IReadOnlyList<DomainEnum.EntityType> GetDependenciesType(this DomainEnum.EntityType type) =>
        type switch
        {
               DomainEnum.EntityType.Muscle => [DomainEnum.EntityType. MuscleAntagonist],
            _ => [],
        };
    
}