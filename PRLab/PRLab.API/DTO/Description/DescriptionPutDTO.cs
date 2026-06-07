using System.ComponentModel.DataAnnotations;
using PRLab.Domain.Utilities;

namespace PRLab.API.DTO.Description;

public record DescriptionPutDTO
{
    public LocalizationHelper.Language? Language { get; set; }

    [Required]
    [StringLength(1024, MinimumLength = 3)]
    public string Content { get; set; } = string.Empty;
}
