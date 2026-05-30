using PRLab.Domain.Model.Entity;

namespace PRLab.Domain.Value.Update;

public class MovementCategoryUpdate
{
    public string? Name { get; set; }

    public DomainEnum.BaseMovementCategory? BaseMovementCategory { get; init; }
    
    public DescriptionUpdate? Description { get; init; }
    
    public User? UpdatedBy { get; init; }
}