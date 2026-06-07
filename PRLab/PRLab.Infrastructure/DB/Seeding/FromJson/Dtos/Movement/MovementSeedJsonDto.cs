using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Movement;

public sealed record MovementSeedJsonDto
{
    public Guid? Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string NameKey { get; init; } = string.Empty;

    public SeedEntityReferenceJsonDto Category { get; init; } = new();

    public DescriptionSeedJsonDto? Description { get; init; }

    public IReadOnlyList<MovementEquipmentRequirementSeedJsonDto> EquipmentRequirements { get; init; } = [];

    public IReadOnlyList<MovementMuscleSeedJsonDto> Muscles { get; init; } = [];

    public DomainEnum.MovementPattern? PrimaryPattern { get; init; }

    public IReadOnlyList<DomainEnum.MovementPattern> Patterns { get; init; } = [];

    public SeedEntityReferenceJsonDto? VariantOf { get; init; }

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
            VariantOf = movement.VariantOf is not null
                ? SeedEntityReferenceJsonDto.FromMovement(movement.VariantOf)
                : null,
            Action = SeedAction.Ignore,
        };
    }
}