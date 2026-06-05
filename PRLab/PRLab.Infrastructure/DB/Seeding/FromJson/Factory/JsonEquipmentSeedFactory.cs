using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Identifier;
using PRLab.Infrastructure.DB.Seeding.FromJson.JsonDtos;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Factory;

public sealed class JsonEquipmentSeedFactory(
    IUserService userService,
    ISeedingConfig config)
    : BaseJsonSeedFactory<Equipment, EquipmentSeedJsonDto>(userService, config),
        IEquipmentSeedFactory
{
   
    protected override DomainEnum.EntityType Entity =>
        DomainEnum.EntityType.Equipment;

    public IReadOnlyList<SeedItem<Equipment>> CreateInitialData()
        => CreateSeedItems();

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

        var description = seedDto.Description is null
            ? Description.None()
            : seedDto.Description.ToDescription();

        var equipment = seedDto.Id.HasValue
            ? Equipment.NewWithId(
                EquipmentId.FromGuid(seedDto.Id.Value),
                seedDto.Name,
                description,
                SeedUser)
            : Equipment.New(
                seedDto.Name,
                description,
                SeedUser);

        return new SeedItem<Equipment>(
            SeedKeyGenerator.GenerateEquipmentKey(equipment),
            equipment,
            seedDto.Action);
    }
}