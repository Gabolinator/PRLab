using PRLab.Domain.Model.Value.Enum.Equipment;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Application.WorkoutImport.Model;

public sealed record WorkoutImportEquipmentReference
{
    public required EquipmentId Id { get; init; }

    public required string Name { get; init; }

    public required string NameKey { get; init; }

    public IReadOnlyList<string> Aliases { get; init; } = [];

    public IReadOnlyCollection<EquipmentConfigurationType> SupportedConfigurationTypes { get; init; } = [];
}
