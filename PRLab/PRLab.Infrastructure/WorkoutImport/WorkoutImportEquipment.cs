using PRLab.Domain.Utilities;

namespace PRLab.Infrastructure.WorkoutImport;

internal static class WorkoutImportEquipment
{
    public static bool SupportsExternalLoad(string name) =>
        !string.IsNullOrWhiteSpace(name) &&
        FormatingUtilities.NormalizeNameKey(name) is "barbell" or "dumbbell" or "kettlebell" or "sandbag";
}
