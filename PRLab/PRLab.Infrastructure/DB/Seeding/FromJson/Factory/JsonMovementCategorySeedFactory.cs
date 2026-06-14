using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory;
using PRLab.Application.Interface.DB.Seeding.Factory.Entity;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Factory;

public class JsonMovementCategorySeedFactory(
    IUserService userService,
    ISeedingConfig config)
    : BaseJsonSeedFactory<MovementCategory, MovementCategorySeedJsonDto>(userService, config),
        IMovementCategorySeedFactory
{
    protected override EntityType Entity =>
        EntityType.MovementCategory;

    public IReadOnlyList<SeedItem<MovementCategory>> CreateInitialData()
    {
        return CreateSeedItems();
    }

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
            ? MovementCategory.NewBuiltInWithId(
                MovementCategoryId.FromGuid(seedDto.Id.Value),
                seedDto.Name,
                seedDto.BaseMovementCategory,
                description,
                SeedUser)
            : MovementCategory.NewBuiltIn(
                seedDto.Name,
                seedDto.BaseMovementCategory,
                description,
                SeedUser);

        return new SeedItem<MovementCategory>(
            SeedKeyGenerator.GenerateMovementCategoryKey(movementCategory),
            movementCategory,
            seedDto.Action);
    }
}