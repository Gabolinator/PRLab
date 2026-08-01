using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory;
using PRLab.Application.Interface.DB.Seeding.Factory.Entity;
using PRLab.Application.Interface.UserService;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;
using PRLab.Infrastructure.DB.Seeding.Validation;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Factory;

public sealed class JsonMovementCategorySeedFactory(
    ISystemUserProvider userService,
    ISeedingConfig config)
    : BaseJsonSeedFactory<MovementCategory, MovementCategorySeedJsonDto>(userService, config),
        IMovementCategorySeedFactory
{
    protected override EntityType Entity =>
        EntityType.MovementCategory;

    public IReadOnlyList<SeedItem<MovementCategory>> CreateInitialData(
        SeedExecutionOptions options)
    {
        return CreateSeedItems(options);
    }

    public override SeedItem<MovementCategory> ToSeedItem(
        SeedExecutionOptions options,
        MovementCategorySeedJsonDto seedDto)
    {
        ArgumentNullException.ThrowIfNull(options);

        Validate(seedDto);

        var description = seedDto.Description is null
            ? Description.None()
            : seedDto.Description.ToDescription();

        var shouldUseSeedId =
            seedDto.Id.HasValue &&
            !options.IgnoreTopLevelIds;

        var movementCategory = shouldUseSeedId
            ? CreateMovementCategoryWithId(seedDto, description)
            : CreateMovementCategory(seedDto, description);

        return new SeedItem<MovementCategory>(
            SeedKeyGenerator.GenerateMovementCategoryKey(movementCategory),
            movementCategory,
            options.ResolveAction(seedDto.Action));
    }

    public override void Validate(MovementCategorySeedJsonDto seedDto)
        => MovementCategorySeedValidator.Validate(seedDto);

    private MovementCategory CreateMovementCategory(
        MovementCategorySeedJsonDto seedDto,
        Description description)
    {
        return seedDto.Origin switch
        {
            DataOrigin.BuiltIn => MovementCategory.NewBuiltIn(
                seedDto.Name,
                seedDto.BaseMovementCategory,
                description,
                SeedUser,
                seedDto.VisibilityScope),

            DataOrigin.UserCreated => MovementCategory.NewUserCreated(
                seedDto.Name,
                seedDto.BaseMovementCategory,
                description,
                GetRequiredOwner(seedDto),
                seedDto.VisibilityScope),

            DataOrigin.Imported => MovementCategory.NewImported(
                seedDto.Name,
                seedDto.BaseMovementCategory,
                description,
                GetRequiredOwner(seedDto),
                seedDto.VisibilityScope),

            DataOrigin.CoachCreated => MovementCategory.NewCoachCreated(
                seedDto.Name,
                seedDto.BaseMovementCategory,
                description,
                GetRequiredOwner(seedDto),
                seedDto.VisibilityScope),

            _ => throw new ArgumentOutOfRangeException(
                nameof(seedDto),
                seedDto.Origin,
                $"{Entity} seed '{seedDto.Name}' has unsupported data origin.")
        };
    }

    private MovementCategory CreateMovementCategoryWithId(
        MovementCategorySeedJsonDto seedDto,
        Description description)
    {
        var id = MovementCategoryId.FromGuid(seedDto.Id!.Value);

        return seedDto.Origin switch
        {
            DataOrigin.BuiltIn => MovementCategory.NewBuiltInWithId(
                id,
                seedDto.Name,
                seedDto.BaseMovementCategory,
                description,
                SeedUser,
                seedDto.VisibilityScope),

            _ => throw new InvalidOperationException(
                $"{Entity} seed '{seedDto.Name}' has a fixed Id but is not BuiltIn. " +
                "Only built-in seed data should use stable seed ids for now.")
        };
    }

    private User GetRequiredOwner(MovementCategorySeedJsonDto seedDto)
    {
        if (!seedDto.OwnerUserId.HasValue || seedDto.OwnerUserId.Value == Guid.Empty)
        {
            throw new InvalidOperationException(
                $"{Entity} seed '{seedDto.Name}' uses origin '{seedDto.Origin}' but has no valid OwnerUserId.");
        }

        /*
         * Temporary seed-side owner.
         *
         * Long term, this should probably resolve an existing User from a UserSeedCatalog
         * or IUserRepository instead of creating a lightweight domain instance.
         */
        return User.Existing(
            UserId.FromGuid(seedDto.OwnerUserId.Value),
            $"Seed Owner {seedDto.OwnerUserId.Value}",
            UserRole.User);
    }
}