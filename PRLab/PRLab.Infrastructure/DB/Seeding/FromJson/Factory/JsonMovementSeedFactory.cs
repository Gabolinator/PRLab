using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory.Entity.Movement;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog;
using PRLab.Application.Models.DB.Seeding.Catalog.Movement;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.Prescription;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Movement; 
using PRLab.Infrastructure.DB.Seeding.FromJson.Relations.Interface;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Factory;

public sealed class JsonMovementSeedFactory(
    IUserService userService,
    ISeedingConfig config,
    IMovementSeedRelationResolver relationResolver)
    : BaseJsonSeedFactory<Movement, MovementSeedJsonDto>(userService, config),
        IMovementSeedFactory, IMovementVariantSeedFactory
{
    protected override EntityType Entity => EntityType.Movement;

    public override SeedItem<Movement> ToSeedItem(MovementSeedJsonDto seedDto)
    {
        throw new NotSupportedException(
            "Movement seeds require equipment, muscle, and movement category catalogs. Use CreateInitialData(...catalogs).");
    }

    public IReadOnlyList<SeedItem<Movement>> CreateInitialData(
        MovementSeedCatalogs catalogs)
    {
        var seedDtos = LoadSeedDtos();

        return seedDtos
            .Select(seedDto => ToSeedItem(seedDto, catalogs))
            .ToList();
    }

    private SeedItem<Movement> ToSeedItem(
        MovementSeedJsonDto seedDto,
        MovementSeedCatalogs catalogs)
    {
        if (string.IsNullOrWhiteSpace(seedDto.Name))
        {
            throw new InvalidOperationException($"{Entity} seed name cannot be empty.");
        }

        if (seedDto.Id == Guid.Empty)
        {
            throw new InvalidOperationException(
                $"{Entity} seed '{seedDto.Name}' has an empty id. Omit the Id property or provide a valid id.");
        }

        ValidateWorkTargetTypes(seedDto);

        var movementCategory = ResolveMovementCategory(
            seedDto.Category,
            catalogs.MovementCategory,
            seedDto.Name);

        var description = seedDto.Description is null
            ? Description.None()
            : seedDto.Description.ToDescription();

        var movement = seedDto.Id.HasValue
            ? Movement.NewBuiltInWithId(
                id: MovementId.FromGuid(seedDto.Id.Value),
                name: seedDto.Name,
                movementCategory: movementCategory,
                description: description,
                defaultWorkTargetType: seedDto.DefaultWorkTargetType,
                allowedWorkTargetTypes: seedDto.AllowedWorkTargetTypes,
                createdBy: SeedUser)
            : Movement.NewBuiltIn(
                name: seedDto.Name,
                movementCategory: movementCategory,
                description: description,
                defaultWorkTargetType: seedDto.DefaultWorkTargetType,
                allowedWorkTargetTypes: seedDto.AllowedWorkTargetTypes,
                createdBy: SeedUser);

        relationResolver.ApplyRelations(
            movement,
            seedDto,
            catalogs,
            SeedUser,
            includeVariant: false);

        return new SeedItem<Movement>(
            SeedKeyGenerator.GenerateMovementKey(movement),
            movement,
            seedDto.Action);
    }

    private static void ValidateWorkTargetTypes(MovementSeedJsonDto seedDto)
    {
        if (seedDto.DefaultWorkTargetType == WorkTargetType.Unspecified)
        {
            throw new InvalidOperationException(
                $"Movement seed '{seedDto.Name}' must provide a DefaultWorkTargetType.");
        }

        if (seedDto.AllowedWorkTargetTypes.Any(targetType => targetType == WorkTargetType.Unspecified))
        {
            throw new InvalidOperationException(
                $"Movement seed '{seedDto.Name}' has an invalid AllowedWorkTargetTypes value.");
        }
    }

    private static MovementCategory ResolveMovementCategory(
        SeedEntityReferenceJsonDto reference,
        MovementCategorySeedCatalog movementCategoryCatalog,
        string movementName)
    {
        if (reference.Id.HasValue)
        {
            return movementCategoryCatalog.GetRequiredById(
                MovementCategoryId.FromGuid(reference.Id.Value));
        }

        if (!string.IsNullOrWhiteSpace(reference.NameKey))
        {
            return movementCategoryCatalog.GetRequiredByNameKey(reference.NameKey);
        }

        if (!string.IsNullOrWhiteSpace(reference.Name))
        {
            return movementCategoryCatalog.GetRequiredByName(reference.Name);
        }

        throw new InvalidOperationException(
            $"Movement seed '{movementName}' must provide a category Id, NameKey, or Name.");
    }

    public IReadOnlyList<SeedRelationItem<MovementId>> CreateVariantInitialData(
        MovementSeedCatalog movementCatalog)
    {
        var seedDtos = LoadSeedDtos();

        var relations = new List<SeedRelationItem<MovementId>>();

        foreach (var seedDto in seedDtos)
        {
            if (seedDto.VariantOf is null)
            {
                continue;
            }

            var sourceMovement = ResolveMovementFromSeedDto(
                seedDto,
                movementCatalog);

            var parentMovement = ResolveMovementReference(
                seedDto.VariantOf,
                movementCatalog);

            relations.Add(
                new SeedRelationItem<MovementId>(
                    sourceMovement.Id,
                    parentMovement.Id,
                    SeedAction.CreateIfMissing));
        }

        return relations;
    }

    private static Movement ResolveMovementFromSeedDto(
        MovementSeedJsonDto seedDto,
        MovementSeedCatalog movementCatalog)
    {
        if (seedDto.Id.HasValue)
        {
            return movementCatalog.GetRequiredById(
                MovementId.FromGuid(seedDto.Id.Value));
        }

        if (!string.IsNullOrWhiteSpace(seedDto.NameKey))
        {
            return movementCatalog.GetRequiredByNameKey(seedDto.NameKey);
        }

        return movementCatalog.GetRequiredByName(seedDto.Name);
    }

    private static Movement ResolveMovementReference(
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