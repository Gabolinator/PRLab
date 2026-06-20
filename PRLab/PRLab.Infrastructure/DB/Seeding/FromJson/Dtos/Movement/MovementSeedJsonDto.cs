using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Value.Enum.Movement;
using PRLab.Domain.Model.Value.Enum.Prescription;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Enum.System;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Movement;

public sealed record MovementSeedJsonDto
{
    public Guid? Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string NameKey { get; init; } = string.Empty;

    public SeedEntityReferenceJsonDto Category { get; init; } = new();

    public DescriptionSeedJsonDto? Description { get; init; }

    public WorkTargetType DefaultWorkTargetType { get; init; }

    public IReadOnlyList<WorkTargetType> AllowedWorkTargetTypes { get; init; } = [];

    public IReadOnlyList<MovementEquipmentRequirementSeedJsonDto> EquipmentRequirements { get; init; } = [];

    public IReadOnlyList<MovementMuscleSeedJsonDto> Muscles { get; init; } = [];

    public MovementPattern? PrimaryPattern { get; init; }

    public IReadOnlyList<MovementPattern> Patterns { get; init; } = [];
    
    public MovementLaterality Laterality { get; init; } = MovementLaterality.Bilateral;

    public SeedEntityReferenceJsonDto? VariantOf { get; init; }

    public DataOrigin Origin { get; init; } = DataOrigin.BuiltIn;

    public Guid? OwnerUserId { get; init; }

    public SeedAction Action { get; init; } = SeedAction.Ignore;

    public static MovementSeedJsonDto FromMovement(Domain.Model.Entity.Movement movement)
    {
        ArgumentNullException.ThrowIfNull(movement);

        return new MovementSeedJsonDto
        {
            Id = movement.Id.Value,
            Name = movement.Name,
            NameKey = movement.NameKey,
            Category = SeedEntityReferenceJsonDto.FromCategory(movement.MovementCategory),
            Description = movement.Description is null
                ? null
                : DescriptionSeedJsonDto.FromDescription(movement.Description),
            DefaultWorkTargetType = movement.DefaultWorkTargetType,
            AllowedWorkTargetTypes = movement.AllowedWorkTargets
                .Select(allowedWorkTarget => allowedWorkTarget.TargetType)
                .OrderBy(targetType => targetType)
                .ToList(),
            EquipmentRequirements = movement.EquipmentRequirements
                .GroupBy(equipmentRequirement => new
                {
                    equipmentRequirement.GroupKey,
                    equipmentRequirement.Kind
                })
                .OrderBy(group => group.Key.Kind)
                .ThenBy(group => group.Key.GroupKey)
                .Select(group => new MovementEquipmentRequirementSeedJsonDto
                {
                    GroupKey = group.Key.GroupKey,
                    Kind = group.Key.Kind,
                    Options = group
                        .Select(equipmentRequirement =>
                            SeedEntityReferenceJsonDto.FromEquipment(equipmentRequirement.Equipment))
                        .OrderBy(equipment => equipment.NameKey)
                        .ToList()
                })
                .ToList(),
            Muscles = movement.Muscles
                .Select(MovementMuscleSeedJsonDto.FromMovementMuscle)
                .OrderBy(muscle => muscle.Role)
                .ThenBy(muscle => muscle.NameKey)
                .ToList(),
            PrimaryPattern = movement.PrimaryPattern,
            Patterns = movement.Patterns
                .Select(pattern => pattern.Pattern)
                .OrderBy(pattern => pattern)
                .ToList(),
            Laterality = movement.Laterality,
            VariantOf = movement.VariantOf is not null
                ? SeedEntityReferenceJsonDto.FromMovement(movement.VariantOf)
                : null,
            Origin = movement.Ownership.Origin,
            OwnerUserId = movement.Ownership.OwnerUserId?.Value,
            Action = SeedAction.Ignore,
        };
    }
}