using PRLab.API.DTO.Movement;
using PRLab.API.DTO.Movement.Relation;
using PRLab.API.DTO.Muscle;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Utilities;

namespace PRLab.API.Mapper;

public static class MovementMapper
{
    public static MovementGetDTO ToGetDTO(Movement movement)
    {
        return ToGetDTO(
            movement,
            (LocalizationHelper.Language?)null);
    }

    public static MovementGetDTO ToGetDTO(
        Movement movement,
        LocalizationHelper.Language? language)
    {
        ArgumentNullException.ThrowIfNull(movement);

        return new MovementGetDTO(
            movement.Id,
            movement.Name,
            DescriptionMapper.ToGetDTO(movement.Description, language),
            movement.MovementCategory is null
                ? null
                : MovementCategoryMapper.ToSummaryDTO(movement.MovementCategory),
            ToEquipmentRequirementGetDTOs(movement.EquipmentRequirements),
            movement.Muscles
                .Where(movementMuscle =>
                    movementMuscle.Role == DomainEnum.MuscleRole.Primary
                    && movementMuscle.Muscle is not null)
                .Select(movementMuscle =>
                    MuscleMapper.ToSummaryDTO(movementMuscle.Muscle))
                .ToList(),
            movement.Muscles
                .Where(movementMuscle =>
                    movementMuscle.Role == DomainEnum.MuscleRole.Secondary
                    && movementMuscle.Muscle is not null)
                .Select(movementMuscle =>
                    MuscleMapper.ToSummaryDTO(movementMuscle.Muscle))
                .ToList(),
            movement.PrimaryPattern,
            movement.Patterns
                .Select(pattern => pattern.Pattern)
                .OrderBy(pattern => pattern)
                .ToList(),
            movement.VariantOf is null
                ? null
                : ToSummaryDTO(movement.VariantOf));
    }

    public static IReadOnlyCollection<MovementGetDTO> ToGetDTOs(
        IReadOnlyCollection<Movement> movements)
    {
        return ToGetDTOs(
            movements,
            (LocalizationHelper.Language?)null);
    }

    public static IReadOnlyCollection<MovementGetDTO> ToGetDTOs(
        IReadOnlyCollection<Movement> movements,
        LocalizationHelper.Language? language)
    {
        ArgumentNullException.ThrowIfNull(movements);

        return movements
            .Select(movement => ToGetDTO(movement, language))
            .ToList();
    }

    public static MovementSummaryDTO ToSummaryDTO(Movement movement)
    {
        ArgumentNullException.ThrowIfNull(movement);

        return new MovementSummaryDTO(
            movement.Id,
            movement.Name);
    }

    public static Movement ToEntity(
        MovementPostDTO payload,
        User? currentUser)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var movement = Movement.New(
            payload.Name,
            payload.MovementCategoryId,
            payload.Descriptor?.Content,
            currentUser);

        ApplyEquipmentRequirements(
            movement,
            payload.EquipmentRequirements,
            currentUser);

        ApplyMuscles(
            movement,
            payload.Muscles,
            currentUser);

        ApplyPatterns(
            movement,
            payload.Patterns,
            payload.PrimaryPattern,
            currentUser);

        if (payload.VariantOfMovementId.HasValue)
        {
            movement.MakeVariantOf(
                payload.VariantOfMovementId.Value,
                currentUser);
        }

        return movement;
    }

    private static IReadOnlyList<MovementEquipmentRequirementGetDTO> ToEquipmentRequirementGetDTOs(
        IReadOnlyCollection<MovementEquipmentRequirement> requirements)
    {
        return requirements
            .GroupBy(requirement => new
            {
                requirement.GroupKey,
                requirement.Kind
            })
            .OrderBy(group => group.Key.Kind)
            .ThenBy(group => group.Key.GroupKey)
            .Select(group =>
                new MovementEquipmentRequirementGetDTO(
                    group.Key.GroupKey,
                    group.Key.Kind,
                    group
                        .Where(requirement => requirement.Equipment is not null)
                        .Select(requirement =>
                            EquipmentMapper.ToSummaryDTO(requirement.Equipment))
                        .OrderBy(equipment => equipment.Name)
                        .ToList()))
            .ToList();
    }

    private static void ApplyEquipmentRequirements(
        Movement movement,
        IReadOnlyCollection<MovementEquipmentRequirementPostDTO>? requirements,
        User? currentUser)
    {
        if (requirements is null)
        {
            return;
        }

        foreach (var requirement in requirements)
        {
            foreach (var equipmentId in requirement.EquipmentIds.Distinct())
            {
                if (requirement.Kind == DomainEnum.EquipmentRequirementKind.Optional)
                {
                    movement.AddOptionalEquipment(
                        equipmentId,
                        requirement.GroupKey,
                        currentUser);
                }
                else
                {
                    movement.AddRequiredEquipmentOption(
                        equipmentId,
                        requirement.GroupKey,
                        currentUser);
                }
            }
        }
    }

    private static void ApplyMuscles(
        Movement movement,
        IReadOnlyCollection<MovementMusclePostDTO>? muscles,
        User? currentUser)
    {
        if (muscles is null)
        {
            return;
        }

        foreach (var muscle in muscles.DistinctBy(muscle => muscle.MuscleId))
        {
            movement.AddMuscle(
                muscle.MuscleId,
                muscle.Role,
                currentUser);
        }
    }

    private static void ApplyPatterns(
        Movement movement,
        IReadOnlyCollection<DomainEnum.MovementPattern>? patterns,
        DomainEnum.MovementPattern? primaryPattern,
        User? currentUser)
    {
        if (patterns is not null)
        {
            foreach (var pattern in patterns.Distinct())
            {
                movement.AddPattern(
                    pattern,
                    currentUser);
            }
        }

        if (primaryPattern.HasValue)
        {
            movement.SetPrimaryPattern(
                primaryPattern.Value,
                currentUser);
        }
        else
        {
            movement.AutoResolvePrimaryPattern(currentUser);
        }
    }
}