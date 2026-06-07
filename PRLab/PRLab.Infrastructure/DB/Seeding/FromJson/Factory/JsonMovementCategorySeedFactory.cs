using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Identifier;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Factory;

public class JsonMovementCategorySeedFactory(
    IUserService userService,
    ISeedingConfig config)
    : BaseJsonSeedFactory<MovementCategory, MovementCategorySeedJsonDto>(userService, config),
        IMovementCategorySeedFactory
{
    protected override DomainEnum.EntityType Entity =>  DomainEnum.EntityType.MovementCategory;
    public override SeedItem<MovementCategory> ToSeedItem(MovementCategorySeedJsonDto seedDto)
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

        var description = seedDto.Description is null
            ? Description.None()
            : seedDto.Description.ToDescription();

        var movementCategory = seedDto.Id.HasValue
            ? MovementCategory.NewWithId(
                MovementCategoryId.FromGuid(seedDto.Id.Value),
                seedDto.Name,
                seedDto.BaseMovementCategory,
                description,
                SeedUser)
            : MovementCategory.New(
                seedDto.Name,
                seedDto.BaseMovementCategory,
                description,
                SeedUser);

        return new SeedItem<MovementCategory>(
            SeedKeyGenerator.GenerateMovementCategoryKey(movementCategory),
            movementCategory,
            seedDto.Action);
    }

    public IReadOnlyList<SeedItem<MovementCategory>> CreateInitialData()
        => CreateSeedItems();
}