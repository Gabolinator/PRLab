using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Utilities;

namespace PRLab.Infrastructure.DB.Seeding;

public static class SeedKeyGenerator
{
    public static string GenerateMovementKey(Movement movement)
    {
        ArgumentNullException.ThrowIfNull(movement);

        return GenerateKey(
            EntityType.Movement,
            movement.NameKey);
    }
    
    public static string GenerateEquipmentKey(Equipment equipment)
    {
        ArgumentNullException.ThrowIfNull(equipment);

        return GenerateKey(
            EntityType.Equipment,
            equipment.NameKey);
    }

    public static string GenerateMuscleKey(Muscle muscle)
    {
        ArgumentNullException.ThrowIfNull(muscle);

        return GenerateKey(
            EntityType.Muscle,
            muscle.NameKey);
    }

    public static string GenerateMovementCategoryKey(MovementCategory movementCategory)
    {
        ArgumentNullException.ThrowIfNull(movementCategory);

        return GenerateKey(
            EntityType.MovementCategory,
            movementCategory.NameKey);
    }
    
    public static string GenerateExerciseKey(Exercise exercise)
    {
        ArgumentNullException.ThrowIfNull(exercise);

        return GenerateKey(
            EntityType.Exercise,
            exercise.NameKey);
    }

    public static string GenerateMuscleKeyFromName(string name)
    {
        return GenerateKey(
            EntityType.Muscle,
            FormatingUtilities.NormalizeNameKey(name));
    }

    public static string GenerateEquipmentKeyFromName(string name)
    {
        return GenerateKey(
            EntityType.Equipment,
            FormatingUtilities.NormalizeNameKey(name));
    }

    public static string GenerateMovementCategoryKeyFromName(string name)
    {
        return GenerateKey(
            EntityType.MovementCategory,
            FormatingUtilities.NormalizeNameKey(name));
    }

    public static string GenerateKey(
        EntityType entityType,
        string nameKey)
    {
        if (string.IsNullOrWhiteSpace(nameKey))
        {
            throw new ArgumentException("Name key cannot be empty.", nameof(nameKey));
        }

        return $"{GetPrefix(entityType)}.{nameKey}";
    }

    private static string GetPrefix(EntityType entityType)
    {
        return entityType switch
        {
            EntityType.Equipment => "equipment",
            EntityType.Muscle => "muscle",
            EntityType.MovementCategory => "movement-category",
            EntityType.Movement => "movement",
            EntityType.Exercise => "exercise",
            _ => throw new ArgumentOutOfRangeException(nameof(entityType), entityType, null)
        };
    }
}