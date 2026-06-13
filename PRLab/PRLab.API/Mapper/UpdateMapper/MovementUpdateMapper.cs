using PRLab.API.DTO.Movement;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Value.Update;

namespace PRLab.API.Mapper.UpdateMapper;

public static class MovementUpdateMapper
{
    public static MovementUpdate ToUpdate(
        Movement movement,
        MovementPutDTO payload,
        User? updatedBy = null)
    {
        ArgumentNullException.ThrowIfNull(movement);
        ArgumentNullException.ThrowIfNull(payload);

        return new MovementUpdate
        {
            Name = payload.Name,
            MovementCategoryId = payload.MovementCategoryId,
            Description = payload.Description is null
                ? null
                : DescriptionUpdateMapper.ToUpdate(payload.Description),
            DefaultWorkTargetType = payload.DefaultWorkTargetType,
            AllowedWorkTargetTypes = payload.AllowedWorkTargetTypes,
            EquipmentRequirements = payload.EquipmentRequirements
                .SelectMany(requirement =>
                    requirement.EquipmentIds
                        .Distinct()
                        .Select(equipmentId =>
                            MovementEquipmentRequirement.New(
                                movementId: movement.Id,
                                equipmentId: equipmentId,
                                groupKey: requirement.GroupKey,
                                kind: requirement.Kind)))
                .ToList(),
            Muscles = payload.Muscles
                .DistinctBy(muscle => muscle.MuscleId)
                .Select(muscle =>
                    MovementMuscle.New(
                        movementId: movement.Id,
                        muscleId: muscle.MuscleId,
                        role: muscle.Role))
                .ToList(),
            PrimaryPattern = payload.PrimaryPattern,
            Patterns = payload.Patterns
                .Distinct()
                .Select(pattern =>
                    MovementPatternTag.New(
                        movementId: movement.Id,
                        pattern: pattern))
                .ToList(),
            VariantOfId = payload.VariantOfMovementId,
            WasVariantOfProvided = true,
            UpdatedBy = updatedBy
        };
    }
}