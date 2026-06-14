using PRLab.API.DTO.Description;
using PRLab.Domain;
using PRLab.Domain.Model.Value.Enum.Movement;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.API.DTO.MovementCategory;


public record MovementCategoryGetDTO(
    MovementCategoryId Id,
    string Name,
    DescriptionGetDTO? Description,
    BaseMovementCategory BaseCategory)
{
}