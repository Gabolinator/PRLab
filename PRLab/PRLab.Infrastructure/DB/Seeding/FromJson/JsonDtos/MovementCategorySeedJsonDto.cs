using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.JsonDtos;

public class MovementCategorySeedJsonDto
{
   public Guid? Id { get; init; }

    public string Name { get; init; } = string.Empty;

    public string NameKey { get; init; } = string.Empty;
    
    public DomainEnum.BaseMovementCategory BaseMovementCategory { get; set; }

    public DescriptionSeedJsonDto? Description { get; init; }

    public SeedAction Action { get; init; } = SeedAction.Ignore;

    public static MovementCategorySeedJsonDto FromMovementCategory(MovementCategory category)
    {
        ArgumentNullException.ThrowIfNull(category);

        return new MovementCategorySeedJsonDto
        {
            Id = category.Id.Value,
            Name = category.Name,
            NameKey = category.NameKey,
            Description = category.Description is null
                ? null
                : DescriptionSeedJsonDto.FromDescription(category.Description),
            Action = SeedAction.Ignore,
        };
    }
    
}