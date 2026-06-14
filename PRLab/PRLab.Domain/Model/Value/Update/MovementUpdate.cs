using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Model.Value.Enum.Movement;
using PRLab.Domain.Model.Value.Enum.Prescription;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities;

namespace PRLab.Domain.Model.Value.Update;

public sealed class MovementUpdate
{
    public string? Name { get; init; }

    public MovementCategoryId? MovementCategoryId { get; init; }

    public DescriptionUpdate? Description { get; init; }

    public WorkTargetType? DefaultWorkTargetType { get; init; }

    public IReadOnlyCollection<WorkTargetType>? AllowedWorkTargetTypes { get; init; }

    public IReadOnlyCollection<MovementEquipmentRequirement>? EquipmentRequirements { get; init; }

    public IReadOnlyCollection<MovementMuscle>? Muscles { get; init; }

    public MovementPattern? PrimaryPattern { get; init; }

    public IReadOnlyCollection<MovementPatternTag>? Patterns { get; init; }

    public MovementId? VariantOfId { get; init; }

    public bool WasVariantOfProvided { get; init; }

    public User? UpdatedBy { get; init; }

    public static MovementUpdate FromMovement(
        Movement movement,
        LocalizationHelper.Language? language,
        User? user)
    {
        ArgumentNullException.ThrowIfNull(movement);

        return new MovementUpdate
        {
            Name = movement.Name,
            MovementCategoryId = movement.MovementCategoryId,
            Description = DescriptionUpdate.FromDescription(
                movement.Description,
                language,
                user),
            DefaultWorkTargetType = movement.DefaultWorkTargetType,
            AllowedWorkTargetTypes = movement.AllowedWorkTargets
                .Select(allowedWorkTarget => allowedWorkTarget.TargetType)
                .ToList(),
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