using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory;
using PRLab.Application.Interface.DB.Seeding.Factory.Entity;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Enum.System;
using PRLab.Domain.Value.Identifier;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Factory;

public sealed class JsonEquipmentSeedFactory(
    IUserService userService,
    ISeedingConfig config)
    : BaseJsonSeedFactory<Equipment, EquipmentSeedJsonDto>(userService, config),
        IEquipmentSeedFactory
{
    protected override EntityType Entity =>
        EntityType.Equipment;

    public IReadOnlyList<SeedItem<Equipment>> CreateInitialData()
    {
        return CreateSeedItems();
    }

    public override SeedItem<Equipment> ToSeedItem(EquipmentSeedJsonDto seedDto)
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

        ValidateOwnership(seedDto);

        var description = seedDto.Description is null
            ? Description.None()
            : seedDto.Description.ToDescription();

        var equipment = seedDto.Id.HasValue
            ? CreateEquipmentWithId(seedDto, description)
            : CreateEquipment(seedDto, description);

        return new SeedItem<Equipment>(
            SeedKeyGenerator.GenerateEquipmentKey(equipment),
            equipment,
            seedDto.Action);
    }

    private Equipment CreateEquipment(
        EquipmentSeedJsonDto seedDto,
        Description description)
    {
        return seedDto.Origin switch
        {
            DataOrigin.BuiltIn => Equipment.NewBuiltIn(
                seedDto.Name,
                description,
                SeedUser),

            DataOrigin.UserCreated => Equipment.NewUserCreated(
                seedDto.Name,
                description,
                GetRequiredOwner(seedDto)),

            DataOrigin.Imported => Equipment.NewImported(
                seedDto.Name,
                description,
                GetRequiredOwner(seedDto)),

            DataOrigin.CoachCreated => Equipment.NewCoachCreated(
                seedDto.Name,
                description,
                GetRequiredOwner(seedDto)),

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
                SeedUser),

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

    private void ValidateOwnership(EquipmentSeedJsonDto seedDto)
    {
        if (seedDto.Origin == DataOrigin.BuiltIn &&
            seedDto.OwnerUserId.HasValue)
        {
            throw new InvalidOperationException(
                $"{Entity} seed '{seedDto.Name}' is BuiltIn and should not have OwnerUserId.");
        }

        if (seedDto.Origin != DataOrigin.BuiltIn &&
            !seedDto.OwnerUserId.HasValue)
        {
            throw new InvalidOperationException(
                $"{Entity} seed '{seedDto.Name}' uses origin '{seedDto.Origin}' and must have OwnerUserId.");
        }
    }
}