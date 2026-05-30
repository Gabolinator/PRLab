using PRLab.Domain.Model.Entity;

namespace PRLab.Domain.Value.Update;

public class MuscleUpdate
{
    public string? Name { get; init; }

    public string? LatinName { get; init; }

    public bool LatinNameWasProvided { get; init; }
    
    public DescriptionUpdate? DescriptionUpdate { get; init; }
    
    public DomainEnum.BodySection? BodySection { get; init; }
    
    public User? UpdatedBy { get; init; }
}