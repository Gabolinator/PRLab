using System.ComponentModel.DataAnnotations;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.PutDto;

public record DescriptionPutDTO
{
    public LocalizationHelper.Language? Language { get; set; }

    [Required]
    [StringLength(1024, MinimumLength = 3)]
    public string Content { get; set; } = string.Empty;
}
