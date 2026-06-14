using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.Movement;
using PRLab.Domain.Utilities;

namespace PRLab.Domain.Model.Value.Update;

public class MovementCategoryUpdate
{
    public string? Name { get; set; }

    public BaseMovementCategory? BaseMovementCategory { get; init; }
    
    public DescriptionUpdate? Description { get; init; }
    
    public User? UpdatedBy { get; init; }

    public static MovementCategoryUpdate FromMovementCategory(
        MovementCategory movementCategory, 
        LocalizationHelper.Language? language, User? user)
        => new()
        {
            Name = movementCategory.Name,
            BaseMovementCategory = movementCategory.BaseMovementCategory,
            Description = DescriptionUpdate.FromDescription(movementCategory.Description, language, user),
            UpdatedBy = user
        };
}