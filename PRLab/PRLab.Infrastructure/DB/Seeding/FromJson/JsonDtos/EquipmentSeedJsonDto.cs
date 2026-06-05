using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain.Model.Entity;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.JsonDtos;

public sealed record EquipmentSeedJsonDto
{
    public Guid? Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string NameKey { get; init; } = string.Empty;

    public DescriptionSeedJsonDto? Description { get; init; }

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
            Action = SeedAction.Ignore,
        };
    }
}