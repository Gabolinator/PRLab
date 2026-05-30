using System.ComponentModel.DataAnnotations;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.PostDto;

public record DescriptionPostDTO
{
    [Required]
    [StringLength(1024, MinimumLength = 3)]
    public string Content { get; init; } = string.Empty;
    
    public LocalizationHelper.Language? Language { get; init; }
}
