using PRLab.Application.Models.DB.Seeding.Catalog;
using PRLab.Application.Models.DB.Seeding.Catalog.Movement;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.Movement;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Movement;
using PRLab.Infrastructure.DB.Seeding.FromJson.Relations.Interface;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Relations;

public sealed class MovementSeedRelationResolver : IMovementSeedRelationResolver
{
    public void ApplyRelations(
        Movement movement,
        MovementSeedJsonDto seedDto,
        MovementSeedCatalogs catalogs,
        User seedUser,
        bool includeVariant = false)
    {
        foreach (var requirement in seedDto.EquipmentRequirements)
        {
            if (string.IsNullOrWhiteSpace(requirement.GroupKey))
            {
                throw new InvalidOperationException(
                    $"Movement seed '{seedDto.Name}' has equipment requirement with empty GroupKey.");
            }

            if (requirement.Options.Count == 0)
            {
                throw new InvalidOperationException(
                    $"Movement seed '{seedDto.Name}' has equipment requirement group '{requirement.GroupKey}' with no options.");
            }

            foreach (var equipmentRef in requirement.Options)
            {
                var equipment = ResolveEquipment(
                    equipmentRef,
                    catalogs.Equipment);

                if (requirement.Kind == EquipmentRequirementKind.Optional)
                {
                    movement.AddOptionalEquipment(
                        equipment.Id,
                        requirement.GroupKey,
                        seedUser);
                }
                else
                {
                    movement.AddRequiredEquipmentOption(
                        equipment.Id,
                        requirement.GroupKey,
                        seedUser);
                }
            }
        }

        foreach (var muscleRef in seedDto.Muscles)
        {
            var muscle = ResolveMuscle(
                muscleRef,
                catalogs.Muscle);

            movement.AddMuscle(
                muscle.Id,
                muscleRef.Role,
                seedUser);
        }

        foreach (var pattern in seedDto.Patterns.Distinct())
        {
            movement.AddPattern(
                pattern,
                seedUser);
        }

        if (seedDto.PrimaryPattern.HasValue)
        {
            movement.SetPrimaryPattern(
                seedDto.PrimaryPattern.Value,
                seedUser);
        }
        else
        {
            movement.AutoResolvePrimaryPattern(seedUser);
        }

        if (includeVariant && seedDto.VariantOf is not null)
        {
            if (catalogs.Movement is null)
            {
                throw new InvalidOperationException(
                    $"Movement seed '{seedDto.Name}' has VariantOf, but no MovementSeedCatalog was provided.");
            }

            var parentMovement = ResolveMovement(
                seedDto.VariantOf,
                catalogs.Movement);

            movement.MakeVariantOf(
                parentMovement.Id,
                seedUser);
        }
    }

    private static Equipment ResolveEquipment(
        SeedEntityReferenceJsonDto reference,
        EquipmentSeedCatalog equipmentCatalog)
    {
        if (reference.Id.HasValue)
        {
            return equipmentCatalog.GetRequiredById(
                EquipmentId.FromGuid(reference.Id.Value));
        }

        if (!string.IsNullOrWhiteSpace(reference.NameKey))
        {
            return equipmentCatalog.GetRequiredByNameKey(reference.NameKey);
        }

        if (!string.IsNullOrWhiteSpace(reference.Name))
        {
            return equipmentCatalog.GetRequiredByName(reference.Name);
        }

        throw new InvalidOperationException(
            "Movement equipment reference must provide Id, NameKey, or Name.");
    }

    private static Muscle ResolveMuscle(
        MovementMuscleSeedJsonDto reference,
        MuscleSeedCatalog muscleCatalog)
    {
        if (reference.Id.HasValue)
        {
            return muscleCatalog.GetRequiredById(
                MuscleId.FromGuid(reference.Id.Value));
        }

        if (!string.IsNullOrWhiteSpace(reference.NameKey))
        {
            return muscleCatalog.GetRequiredByNameKey(reference.NameKey);
        }

        if (!string.IsNullOrWhiteSpace(reference.Name))
        {
            return muscleCatalog.GetRequiredByName(reference.Name);
        }

        throw new InvalidOperationException(
            "Movement muscle reference must provide Id, NameKey, or Name.");
    }

    private static Movement ResolveMovement(
        SeedEntityReferenceJsonDto reference,
        MovementSeedCatalog movementCatalog)
    {
        if (reference.Id.HasValue)
        {
            return movementCatalog.GetRequiredById(
                MovementId.FromGuid(reference.Id.Value));
        }

        if (!string.IsNullOrWhiteSpace(reference.NameKey))
        {
            return movementCatalog.GetRequiredByNameKey(reference.NameKey);
        }

        if (!string.IsNullOrWhiteSpace(reference.Name))
        {
            return movementCatalog.GetRequiredByName(reference.Name);
        }

        throw new InvalidOperationException(
            "Movement variant reference must provide Id, NameKey, or Name.");
    }
}