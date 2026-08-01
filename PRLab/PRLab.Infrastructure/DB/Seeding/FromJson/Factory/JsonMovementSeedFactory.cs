using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory.Entity.Movement;
using PRLab.Application.Interface.UserService;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog;
using PRLab.Application.Models.DB.Seeding.Catalog.Movement;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Movement; 
using PRLab.Infrastructure.DB.Seeding.FromJson.Relations.Interface;
using PRLab.Infrastructure.DB.Seeding.Validation;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Factory;

public sealed class JsonMovementSeedFactory(
    ISystemUserProvider userService,
    ISeedingConfig config,
    IMovementSeedRelationResolver relationResolver)
    : BaseJsonSeedFactory<Movement, MovementSeedJsonDto>(userService, config),
        IMovementSeedFactory, IMovementVariantSeedFactory
{
    protected override EntityType Entity => EntityType.Movement;

    public override SeedItem<Movement> ToSeedItem(SeedExecutionOptions options, MovementSeedJsonDto seedDto)
    {
        throw new NotSupportedException(
            "Movement seeds require equipment, muscle, and movement category catalogs. Use CreateInitialData(...catalogs).");
    }

    public override void Validate(MovementSeedJsonDto seedDto)
        => MovementSeedValidator.Validate(seedDto);
    
    public IReadOnlyList<SeedItem<Movement>> CreateInitialData(
        SeedExecutionOptions options,
        MovementSeedCatalogs catalogs)
    {
        var seedDtos = LoadSeedDtos();

        return seedDtos
            .Select(seedDto => ToSeedItem(options, seedDto, catalogs))
            .ToList();
    }
    
    
    private static MovementCategory ResolveMovementCategory(
        SeedExecutionOptions options,
        SeedEntityReferenceJsonDto reference,
        MovementCategorySeedCatalog movementCategoryCatalog,
        string movementName)
    {
        if (reference.Id.HasValue && !options.IgnoreReferenceIds)
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

    private SeedItem<Movement> ToSeedItem(
        SeedExecutionOptions options,
        MovementSeedJsonDto seedDto,
        MovementSeedCatalogs catalogs)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(seedDto);
        ArgumentNullException.ThrowIfNull(catalogs);

        Validate(seedDto);

        var movementCategory = ResolveMovementCategory(
            options,
            seedDto.Category,
            catalogs.MovementCategory,
            seedDto.Name);

        var description = seedDto.Description is null
            ? Description.None()
            : seedDto.Description.ToDescription();

        var shouldUseSeedId =
            seedDto.Id.HasValue &&
            !options.IgnoreTopLevelIds;

        var movement = shouldUseSeedId
            ? Movement.NewBuiltInWithId(
                id: MovementId.FromGuid(seedDto.Id!.Value),
                name: seedDto.Name,
                movementCategory: movementCategory,
                description: description,
                defaultWorkTargetType: seedDto.DefaultWorkTargetType,
                laterality: seedDto.Laterality,
                allowedWorkTargetTypes: seedDto.AllowedWorkTargetTypes,
                createdBy: SeedUser,
                visibilityScope: seedDto.VisibilityScope)
            : Movement.NewBuiltIn(
                name: seedDto.Name,
                movementCategory: movementCategory,
                description: description,
                defaultWorkTargetType: seedDto.DefaultWorkTargetType,
                laterality: seedDto.Laterality,
                allowedWorkTargetTypes: seedDto.AllowedWorkTargetTypes,
                createdBy: SeedUser,
                visibilityScope: seedDto.VisibilityScope);

        relationResolver.ApplyRelations(
            movement,
            seedDto,
            catalogs,
            SeedUser,
            options,
            includeVariant: false);

        return new SeedItem<Movement>(
            SeedKeyGenerator.GenerateMovementKey(movement),
            movement,
            options.ResolveAction(seedDto.Action));
    }

    public IReadOnlyList<SeedRelationItem<MovementId>> CreateVariantInitialData(
        SeedExecutionOptions options,
        MovementSeedCatalog movementCatalog)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(movementCatalog);

        var seedDtos = LoadSeedDtos();

        var relations = new List<SeedRelationItem<MovementId>>();

        foreach (var seedDto in seedDtos)
        {
            if (seedDto.VariantOf is null)
            {
                continue;
            }

            var sourceMovement = ResolveMovementFromSeedDto(
                options,
                seedDto,
                movementCatalog);

            var parentMovement = ResolveMovementReference(
                options,
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
        SeedExecutionOptions options,
        MovementSeedJsonDto seedDto,
        MovementSeedCatalog movementCatalog)
    {
        if (seedDto.Id.HasValue && !options.IgnoreTopLevelIds)
        {
            return movementCatalog.GetRequiredById(
                MovementId.FromGuid(seedDto.Id.Value));
        }

        if (!string.IsNullOrWhiteSpace(seedDto.NameKey))
        {
            return movementCatalog.GetRequiredByNameKey(seedDto.NameKey);
        }

        if (!string.IsNullOrWhiteSpace(seedDto.Name))
        {
            return movementCatalog.GetRequiredByName(seedDto.Name);
        }

        throw new InvalidOperationException(
            "Movement seed must provide Id, NameKey, or Name.");
    }

    private static Movement ResolveMovementReference(
        SeedExecutionOptions options,
        SeedEntityReferenceJsonDto reference,
        MovementSeedCatalog movementCatalog)
    {
        if (reference.Id.HasValue && !options.IgnoreReferenceIds)
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