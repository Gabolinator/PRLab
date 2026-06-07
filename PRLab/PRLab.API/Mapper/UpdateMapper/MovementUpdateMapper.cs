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
        User? currentUser)
    {
        ArgumentNullException.ThrowIfNull(movement);
        ArgumentNullException.ThrowIfNull(payload);

        return new MovementUpdate
        {
            Name = payload.Name,
            MovementCategoryId = payload.MovementCategoryId,
            Description = DescriptionUpdateMapper.ToUpdate(payload.Description),
            EquipmentRequirements = payload.EquipmentRequirements
                .SelectMany(requirement =>
                    requirement.EquipmentIds
                        .Distinct()
                        .Select(equipmentId =>
                            MovementEquipmentRequirement.New(
                                movement.Id,
                                equipmentId,
                                requirement.GroupKey,
                                requirement.Kind)))
                .ToList(),
            Muscles = payload.Muscles
                .DistinctBy(muscle => muscle.MuscleId)
                .Select(muscle =>
                    MovementMuscle.New(
                        movement.Id,
                        muscle.MuscleId,
                        muscle.Role))
                .ToList(),
            PrimaryPattern = payload.PrimaryPattern,
            Patterns = payload.Patterns
                .Distinct()
                .Select(pattern =>
                    MovementPatternTag.New(
                        movement.Id,
                        pattern))
                .ToList(),
            VariantOfId = payload.VariantOfMovementId,
            WasVariantOfProvided = true,
            UpdatedBy = currentUser,
        };
    }
}