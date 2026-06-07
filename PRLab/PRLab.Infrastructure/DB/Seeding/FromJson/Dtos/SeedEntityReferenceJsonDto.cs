using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Join;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;

public sealed record SeedEntityReferenceJsonDto
{
    public Guid? Id { get; init; }

    public string? Name { get; init; }

    public string? NameKey { get; init; }

    public static SeedEntityReferenceJsonDto FromCategory(MovementCategory entity) =>
        new()
        {
            Id = entity.Id.Value,
            Name = entity.Name,
            NameKey = entity.NameKey,
        };

    public static SeedEntityReferenceJsonDto FromMuscle(Domain.Model.Entity.Muscle entity) =>
        new()
        {
            Id = entity.Id.Value,
            Name = entity.Name,
            NameKey = entity.NameKey,
        };

    public static SeedEntityReferenceJsonDto FromMovement(Domain.Model.Entity.Movement entity) =>
        new()
        {
            Id = entity.Id.Value,
            Name = entity.Name,
            NameKey = entity.NameKey,
        };

    public static SeedEntityReferenceJsonDto FromEquipment(Equipment entity) =>
        new()
        {
            Id = entity.Id.Value,
            Name = entity.Name,
            NameKey = entity.NameKey,
        };

    public static SeedEntityReferenceJsonDto FromMovementEquipment(MovementEquipment entity)
        =>
            new()
            {
                Id = entity.EquipmentId.Value,
                Name = entity.Equipment.Name,
                NameKey = entity.Equipment.NameKey,
            };
}