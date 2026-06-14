using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.System;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;

public sealed record EquipmentSeedJsonDto
{
    public Guid? Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string NameKey { get; init; } = string.Empty;

    public DescriptionSeedJsonDto? Description { get; init; }

    public DataOrigin Origin { get; init; } = DataOrigin.BuiltIn;

    public Guid? OwnerUserId { get; init; }

    public SeedAction Action { get; init; } = SeedAction.Ignore;

    public static EquipmentSeedJsonDto FromEquipment(Equipment equipment)
    {
        ArgumentNullException.ThrowIfNull(equipment);

        return new EquipmentSeedJsonDto
        {
            Id = equipment.Id.Value,
            Name = equipment.Name,
            NameKey = equipment.NameKey,
            Description = equipment.Description is null
                ? null
                : DescriptionSeedJsonDto.FromDescription(equipment.Description),
            Origin = equipment.Ownership.Origin,
            OwnerUserId = equipment.Ownership.OwnerUserId?.Value,
            Action = SeedAction.Ignore,
        };
    }
}