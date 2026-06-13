using PRLab.Domain;
using PRLab.Domain.Value.Enum.Movement;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;

public sealed record MovementEquipmentRequirementSeedJsonDto
{
    public string GroupKey { get; init; } = string.Empty;

    public EquipmentRequirementKind Kind { get; init; }

    public IReadOnlyList<SeedEntityReferenceJsonDto> Options { get; init; } = [];
    
}