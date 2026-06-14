using PRLab.API.DTO.MovementCategory;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Update;

namespace PRLab.API.Mapper.UpdateMapper;

public static class MovementCategoryUpdateMapper
{
    public static MovementCategoryUpdate ToUpdate(
        MovementCategoryPutDTO payload,
        User? updatedBy = null)
    {
        ArgumentNullException.ThrowIfNull(payload);

        return new MovementCategoryUpdate
        {
            Name = payload.Name,
            BaseMovementCategory = payload.BaseMovementCategory,
            Description = payload.Description is null
                ? null
                : DescriptionUpdateMapper.ToUpdate(payload.Description),
            UpdatedBy = updatedBy,
        };
    }
}