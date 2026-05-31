using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities;

namespace PRLab.Domain.Value.Update;

public sealed record EquipmentUpdate()
{
    public string? Name { get; init; }

    public DescriptionUpdate? DescriptionUpdate { get; init; }
    
    public User? UpdatedBy { get; init; }

    public static EquipmentUpdate FromEquipment(Equipment equipment, LocalizationHelper.Language? language ,User? updatedBy)
        => new()
        {
            Name = equipment.Name,
            DescriptionUpdate = DescriptionUpdate.FromDescription(equipment.Description, language ,updatedBy),
            UpdatedBy = updatedBy
        };
    
}