using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.GetDto;

public record DescriptorGetDTO(
    DescriptionId Id, 
    string Content, 
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    long UpdatedSeq,
    bool IsDeleted = false,
    DataAuthority Authority = DataAuthority.Bidirectional );