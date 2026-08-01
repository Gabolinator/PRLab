using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory.Entity;
using PRLab.Application.Interface.UserService;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;
using PRLab.Infrastructure.DB.Seeding.Validation;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Factory;

public sealed class JsonEquipmentSeedFactory(
    ISystemUserProvider userService,
    ISeedingConfig config)
    : BaseJsonSeedFactory<Equipment, EquipmentSeedJsonDto>(userService, config),
        IEquipmentSeedFactory
{
    protected override EntityType Entity =>
        EntityType.Equipment;

    public IReadOnlyList<SeedItem<Equipment>> CreateInitialData(SeedExecutionOptions options)
    {
        return CreateSeedItems(options);
    }

    public override SeedItem<Equipment> ToSeedItem(SeedExecutionOptions options, EquipmentSeedJsonDto seedDto)
    {
        Validate(seedDto);
        
        var description = seedDto.Description is null
            ? Description.None()
            : seedDto.Description.ToDescription();

        var shouldUseSeedId =
            seedDto.Id.HasValue &&
            !options.IgnoreTopLevelIds;

        var equipment = shouldUseSeedId
            ? CreateEquipmentWithId(seedDto, description)
            : CreateEquipment(seedDto, description);

        return new SeedItem<Equipment>(
            SeedKeyGenerator.GenerateEquipmentKey(equipment),
            equipment,
            options.ResolveAction(seedDto.Action));
    }

    public override void Validate(EquipmentSeedJsonDto seedDto)
        => EquipmentSeedValidator.Validate(seedDto);

    private Equipment CreateEquipment(
        EquipmentSeedJsonDto seedDto,
        Description description)
    {
        return seedDto.Origin switch
        {
            DataOrigin.BuiltIn => Equipment.NewBuiltIn(
                seedDto.Name,
                description,
                SeedUser,
                seedDto.VisibilityScope),

            DataOrigin.UserCreated => Equipment.NewUserCreated(
                seedDto.Name,
                description,
                GetRequiredOwner(seedDto),
                seedDto.VisibilityScope),

            DataOrigin.Imported => Equipment.NewImported(
                seedDto.Name,
                description,
                GetRequiredOwner(seedDto),
                seedDto.VisibilityScope),

            DataOrigin.CoachCreated => Equipment.NewCoachCreated(
                seedDto.Name,
                description,
                GetRequiredOwner(seedDto),
                seedDto.VisibilityScope),

            _ => throw new ArgumentOutOfRangeException(
                nameof(seedDto),
                seedDto.Origin,
                $"{Entity} seed '{seedDto.Name}' has unsupported data origin.")
        };
    }

    private Equipment CreateEquipmentWithId(
        EquipmentSeedJsonDto seedDto,
        Description description)
    {
        var id = EquipmentId.FromGuid(seedDto.Id!.Value);

        return seedDto.Origin switch
        {
            DataOrigin.BuiltIn => Equipment.NewBuiltInWithId(
                id,
                seedDto.Name,
                description,
                SeedUser,
                seedDto.VisibilityScope),

            _ => throw new InvalidOperationException(
                $"{Entity} seed '{seedDto.Name}' has a fixed Id but is not BuiltIn. " +
                "Only built-in seed data should use stable seed ids for now.")
        };
    }

    private User GetRequiredOwner(EquipmentSeedJsonDto seedDto)
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