using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities;

namespace PRLab.Infrastructure.DB.Seeding;

public static class SeedKeyGenerator
{
    public static string GenerateMovementKey(Movement movement)
    {
        ArgumentNullException.ThrowIfNull(movement);

        return GenerateKey(
            DomainEnum.EntityType.Movement,
            movement.NameKey);
    }
    
    public static string GenerateEquipmentKey(Equipment equipment)
    {
        ArgumentNullException.ThrowIfNull(equipment);

        return GenerateKey(
            DomainEnum.EntityType.Equipment,
            equipment.NameKey);
    }

    public static string GenerateMuscleKey(Muscle muscle)
    {
        ArgumentNullException.ThrowIfNull(muscle);

        return GenerateKey(
            DomainEnum.EntityType.Muscle,
            muscle.NameKey);
    }

    public static string GenerateMovementCategoryKey(MovementCategory movementCategory)
    {
        ArgumentNullException.ThrowIfNull(movementCategory);

        return GenerateKey(
            DomainEnum.EntityType.MovementCategory,
            movementCategory.NameKey);
    }

    public static string GenerateMuscleKeyFromName(string name)
    {
        return GenerateKey(
            DomainEnum.EntityType.Muscle,
            FormatingUtilities.NormalizeNameKey(name));
    }

    public static string GenerateEquipmentKeyFromName(string name)
    {
        return GenerateKey(
            DomainEnum.EntityType.Equipment,
            FormatingUtilities.NormalizeNameKey(name));
    }

    public static string GenerateMovementCategoryKeyFromName(string name)
    {
        return GenerateKey(
            DomainEnum.EntityType.MovementCategory,
            FormatingUtilities.NormalizeNameKey(name));
    }

    public static string GenerateKey(
        DomainEnum.EntityType entityType,
        string nameKey)
    {
        if (string.IsNullOrWhiteSpace(nameKey))
        {
            throw new ArgumentException("Name key cannot be empty.", nameof(nameKey));
        }

        return $"{GetPrefix(entityType)}.{nameKey}";
    }

    private static string GetPrefix(DomainEnum.EntityType entityType)
    {
        return entityType switch
        {
            DomainEnum.EntityType.Equipment => "equipment",
            DomainEnum.EntityType.Muscle => "muscle",
            DomainEnum.EntityType.MovementCategory => "movement-category",
            DomainEnum.EntityType.Movement => "movement",
            _ => throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null)
        };
    }
}