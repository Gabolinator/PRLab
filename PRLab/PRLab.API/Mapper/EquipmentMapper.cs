using PRLab.API.DTO.Equipment;
using PRLab.API.DTO.Muscle;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities;

namespace PRLab.API.Mapper;

public static class EquipmentMapper
{
    public static EquipmentGetDTO ToGetDTO(Equipment equipment)
    {
        return ToGetDTO(
            equipment,
            (LocalizationHelper.Language?)null
        );
    }

    public static EquipmentGetDTO ToGetDTO(
        Equipment equipment,
        LocalizationHelper.Language? language)
    {
        ArgumentNullException.ThrowIfNull(equipment);

        return new EquipmentGetDTO(
            equipment.Id,
            equipment.Name,
            DescriptionMapper.ToGetDTO(equipment.Description, language)
        );
    }

    public static IReadOnlyCollection<EquipmentGetDTO> ToGetDTOs(
        IReadOnlyCollection<Equipment> equipments)
    {
        return ToGetDTOs(
            equipments,
            (LocalizationHelper.Language?)null
        );
    }

    public static IReadOnlyCollection<EquipmentGetDTO> ToGetDTOs(
        IReadOnlyCollection<Equipment> equipments,
        LocalizationHelper.Language? language)
    {
        ArgumentNullException.ThrowIfNull(equipments);

        return equipments
            .Select(equipment => ToGetDTO(equipment, language))
            .ToList();
    }

    public static Equipment ToEntity(
        EquipmentPostDTO payload,
        User currentUser)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(currentUser);

        var description = payload.Descriptor is null
            ? Description.New(null)
            : DescriptionMapper.ToEntity(payload.Descriptor);

        return Equipment.NewUserCreated(
            payload.Name,
            description,
            currentUser
        );
    }

    public static EquipmentSummaryDTO ToSummaryDTO(Equipment equipment) =>
        new(equipment.Id, equipment.Name);
}