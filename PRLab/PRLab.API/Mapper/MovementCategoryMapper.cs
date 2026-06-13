using PRLab.API.DTO.MovementCategory;
using PRLab.API.DTO.Muscle;
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
        MovementCategoryPostDTO payload,
        User createdBy)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(createdBy);

        var description = payload.Description is null
            ? Description.New(null)
            : DescriptionMapper.ToEntity(payload.Description);

        return MovementCategory.NewUserCreated(
            payload.Name,
            payload.BaseCategory,
            description,
            createdBy);
    }
    
    public static MovementCategorySummaryDTO? ToSummaryDTO(MovementCategory category) =>
        new(category.Id, category.Name);
}