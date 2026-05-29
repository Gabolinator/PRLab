using PRLab.API.Dtos.GetDto;
using PRLab.API.Dtos.PostDto;
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
            DescriptionMapper.ToGetDTO(equipment.Description, language),
            equipment.Audit.CreatedAt.ToUniversalTime(),
            (equipment.Audit.UpdatedAt ?? equipment.Audit.CreatedAt).ToUniversalTime(),
            equipment.Audit.IsDeleted,
            DataAuthority.Bidirectional
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

    public static Equipment ToEntity(EquipmentPostDTO payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var description = payload.Descriptor is null
            ? Description.New(null)
            : DescriptionMapper.ToEntity(payload.Descriptor);

        return Equipment.New(
            payload.Name,
            description
        );
    }
}