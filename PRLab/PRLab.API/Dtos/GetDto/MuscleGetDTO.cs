using PRLab.API.Dtos.SummaryDto;
using PRLab.Domain;
using PRLab.Domain.Value.Identifier;

namespace PRLab.API.Dtos.GetDto;

/// <summary>
/// Response DTO describing a muscle, its descriptor metadata, and relationships.
/// </summary>
/// <param name="Id">Stable identifier of the muscle.</param>
/// <param name="Name">Display name.</param>
/// <param name="LatinName">Optional Latin/scientific name.</param>
/// <param name="BodySection">Body section classification.</param>
/// <param name="DescriptorId">Linked descriptor identifier.</param>
/// <param name="Descriptor">Descriptor payload projected with the muscle.</param>
/// <param name="AntagonistIds">Ids of muscles classified as antagonists.</param>
/// <param name="CreatedAtUtc">Creation timestamp tracked on the server.</param>
/// <param name="UpdatedAtUtc">Last update timestamp.</param>
/// <param name="UpdatedSeq">Monotonic sequence used for sync.</param>
/// <param name="IsDeleted">Marks soft-deleted records.</param>
/// <param name="Authority">Authority that owns the record.</param>
public sealed record MuscleGetDTO(
    MuscleId Id,
    string Name,
    string? LatinName,
    DomainEnum.BodySection BodySection,
    DescriptionSummaryDTO? Descriptor,
    IReadOnlyList<MuscleSummaryDTO>? Antagonists,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    long UpdatedSeq,
    bool IsDeleted = false,
    DataAuthority Authority = DataAuthority.Bidirectional);
    