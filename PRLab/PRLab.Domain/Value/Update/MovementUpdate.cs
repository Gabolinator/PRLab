using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Utilities;

namespace PRLab.Domain.Value.Update;

public class MovementUpdate
{
    public string? Name { get; private set; }
    public User? UpdatedBy { get; private set; }
    public DescriptionUpdate? Description { get; private set; }
    public DomainEnum.MovementPattern? PrimaryPattern { get; private set; }
    public IReadOnlyCollection<MovementPatternTag>? Patterns { get; private set; }
    public IReadOnlyCollection<MovementMuscle>? Muscles { get; private set; }
    
    public MovementCategory? MovementCategory { get; private set; }
    
    public Movement? VariantOf { get; private set; }
    
    public IReadOnlyCollection<Movement>? Variants  { get; private set; }
    public IReadOnlyCollection<MovementEquipmentRequirement>? Equipments { get; private set; }
    public bool WasVariantOfProvided { get; private set; }
    
    public static MovementUpdate FromMovement(
        Movement movement,
        LocalizationHelper.Language? language,
        User updatedBy)
        => new()
        {
            UpdatedBy = updatedBy,
            Name = movement.Name,
            Description = DescriptionUpdate.FromDescription(
                movement.Description,
                language,
                updatedBy),
            PrimaryPattern = movement.PrimaryPattern,
            Patterns = movement.Patterns,
            Muscles = movement.Muscles,
            Equipments = movement.EquipmentRequirements,
            MovementCategory = movement.MovementCategory,
            Variants = movement.Variants,
            VariantOf = movement.VariantOf,
            WasVariantOfProvided = true,
        };

    
}