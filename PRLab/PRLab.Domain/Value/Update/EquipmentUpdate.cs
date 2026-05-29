using PRLab.Domain.Model.Entity;

namespace PRLab.Domain.Value.Update;

public sealed record EquipmentUpdate()
{
    public string? Name { get; init; }

    public DescriptionUpdate? DescriptionUpdate { get; init; }
    
    public User? UpdatedBy { get; init; }
}