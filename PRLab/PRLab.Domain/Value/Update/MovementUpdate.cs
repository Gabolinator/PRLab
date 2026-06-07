using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Domain.Value.Update;

public sealed class MovementUpdate
{
    public string? Name { get; init; }

    public MovementCategoryId? MovementCategoryId { get; init; }

    public DescriptionUpdate? Description { get; init; }

    public IReadOnlyCollection<MovementEquipmentRequirement>? EquipmentRequirements { get; init; }

    public IReadOnlyCollection<MovementMuscle>? Muscles { get; init; }

    public DomainEnum.MovementPattern? PrimaryPattern { get; init; }

    public IReadOnlyCollection<MovementPatternTag>? Patterns { get; init; }

    public MovementId? VariantOfId { get; init; }

    public bool WasVariantOfProvided { get; init; }

    public User? UpdatedBy { get; init; }

    public static MovementUpdate FromMovement(
        Movement movement,
        LocalizationHelper.Language? language,
        User? user)
    {
        return new MovementUpdate
        {
            Name = movement.Name,
            MovementCategoryId = movement.MovementCategoryId,
            Description = DescriptionUpdate.FromDescription(
                movement.Description,
                language,
                user),
            EquipmentRequirements = movement.EquipmentRequirements,
            Muscles = movement.Muscles,
            PrimaryPattern = movement.PrimaryPattern,
            Patterns = movement.Patterns,
            VariantOfId = movement.VariantOfId,
            WasVariantOfProvided = true,
            UpdatedBy = user
        };
    }
}