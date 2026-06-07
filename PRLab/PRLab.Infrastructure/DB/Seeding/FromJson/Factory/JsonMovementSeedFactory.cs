using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog;
using PRLab.Application.Models.DB.Seeding.Catalog.Movement;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Identifier;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos.Movement; 
using PRLab.Infrastructure.DB.Seeding.FromJson.Relations.Interface;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Factory;

public sealed class JsonMovementSeedFactory(
    IUserService userService,
    ISeedingConfig config,
    IMovementSeedRelationResolver relationResolver)
    : BaseJsonSeedFactory<Movement, MovementSeedJsonDto>(userService, config),
        IMovementSeedFactory
{
    protected override DomainEnum.EntityType Entity => DomainEnum.EntityType.Movement;

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

        var movementCategory = ResolveMovementCategory(
            seedDto.Category,
            catalogs.MovementCategory,
            seedDto.Name);

        var description = seedDto.Description is null
            ? Description.None()
            : seedDto.Description.ToDescription();

        var movement = seedDto.Id.HasValue
            ? Movement.NewWithId(
                MovementId.FromGuid(seedDto.Id.Value),
                seedDto.Name,
                movementCategory,
                description,
                SeedUser)
            : Movement.New(
                seedDto.Name,
                movementCategory,
                description,
                SeedUser);

        relationResolver.ApplyRelations(
            movement,
            seedDto,
            catalogs,
            SeedUser);

        return new SeedItem<Movement>(
            SeedKeyGenerator.GenerateMovementKey(movement),
            movement,
            seedDto.Action);
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
}