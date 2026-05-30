using PRLab.API.Dtos.GetDto;
using PRLab.API.Dtos.PostDto;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities;

namespace PRLab.API.Mapper;

public static class MovementCategoryMapper
{
    public static MovementCategoryGetDTO ToGetDTO(
        MovementCategory movementCategory)
    {
        return ToGetDTO(
            movementCategory,
            (LocalizationHelper.Language?)null);
    }

    public static MovementCategoryGetDTO ToGetDTO(
        MovementCategory movementCategory,
        LocalizationHelper.Language? language)
    {
        ArgumentNullException.ThrowIfNull(movementCategory);

        return new MovementCategoryGetDTO(
            movementCategory.Id,
            movementCategory.Name,
            DescriptionMapper.ToGetDTO(movementCategory.Description, language),
            movementCategory.BaseMovementCategory);
    }

    public static IReadOnlyCollection<MovementCategoryGetDTO> ToGetDTOs(
        IReadOnlyCollection<MovementCategory> movementCategories)
    {
        return ToGetDTOs(
            movementCategories,
            (LocalizationHelper.Language?)null);
    }

    public static IReadOnlyCollection<MovementCategoryGetDTO> ToGetDTOs(
        IReadOnlyCollection<MovementCategory> movementCategories,
        LocalizationHelper.Language? language)
    {
        ArgumentNullException.ThrowIfNull(movementCategories);

        return movementCategories
            .Select(movementCategory => ToGetDTO(movementCategory, language))
            .ToList();
    }

    public static MovementCategory ToEntity(
        MovementCategoryPostDTO payload)
    {
        return ToEntity(payload, null);
    }

    public static MovementCategory ToEntity(
        MovementCategoryPostDTO payload,
        User? createdBy)
    {
        ArgumentNullException.ThrowIfNull(payload);

        return MovementCategory.New(
            payload.Name,
            payload.Description?.Content,
            payload.BaseCategory,
            createdBy);
    }
}