using PRLab.Domain;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;

public sealed record MovementEquipmentRequirementSeedJsonDto
{
    public string GroupKey { get; init; } = string.Empty;

    public DomainEnum.EquipmentRequirementKind Kind { get; init; }

    public IReadOnlyList<SeedEntityReferenceJsonDto> Options { get; init; } = [];
    
}