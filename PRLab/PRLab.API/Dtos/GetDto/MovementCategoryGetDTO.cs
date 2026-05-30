using PRLab.API.Dtos.SummaryDto;
using PRLab.Domain;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.GetDto;


public record MovementCategoryGetDTO(
    MovementCategoryId Id,
    string Name,
    DescriptionGetDTO? Description,
    DomainEnum.BaseMovementCategory BaseCategory)
{
}