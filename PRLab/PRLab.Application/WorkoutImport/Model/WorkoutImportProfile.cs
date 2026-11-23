using System.Text.Json.Serialization;
using PRLab.Domain.Model.Value.Enum.Workout;

namespace PRLab.Application.WorkoutImport.Model;

public sealed class WorkoutImportProfile
{
    public required string Name { get; init; }

    public IReadOnlyList<WorkoutImportSourceProfile> SourceProfiles { get; init; } = [];

    public IReadOnlyDictionary<string, string> ExerciseAliases { get; init; }
        = new Dictionary<string, string>();

    public IReadOnlyDictionary<string, WorkoutBlockType> BlockMappings { get; init; }
        = new Dictionary<string, WorkoutBlockType>();

    public IReadOnlyList<string> IgnoredLabels { get; init; } = [];

    public string? InterpretationInstructions { get; init; }

    public TProfile GetRequiredSourceProfile<TProfile>()
        where TProfile : WorkoutImportSourceProfile
    {
        var profile = SourceProfiles
            .OfType<TProfile>()
            .SingleOrDefault();

        return profile
               ?? throw new InvalidOperationException(
                   $"Workout import profile '{Name}' does not contain " +
                   $"a source profile of type '{typeof(TProfile).Name}'.");
    }

    public bool TryGetSourceProfile<TProfile>(
        out TProfile? profile)
        where TProfile : WorkoutImportSourceProfile
    {
        profile = SourceProfiles
            .OfType<TProfile>()
            .SingleOrDefault();

        return profile is not null;
    }
}

[JsonPolymorphic(TypeDiscriminatorPropertyName = "Type")]
[JsonDerivedType(
    typeof(ExcelWorkoutImportSourceProfile),
    typeDiscriminator: "Excel")]
public abstract class WorkoutImportSourceProfile
{
    public abstract WorkoutImportSourceFormat Format { get; }
}

public sealed class ExcelWorkoutImportSourceProfile
    : WorkoutImportSourceProfile
{
    public override WorkoutImportSourceFormat Format =>
        WorkoutImportSourceFormat.Excel;

    public IReadOnlyList<ExcelWorkoutImportColumnMapping> Columns { get; init; } = [];

    public bool TryGetColumn(
        WorkoutImportProperty property,
        out int columnIndex)
    {
        var mapping = Columns
            .SingleOrDefault(column =>
                column.Property == property);

        if (mapping is null)
        {
            columnIndex = -1;
            return false;
        }

        columnIndex = mapping.CellIndex;
        return true;
    }

    public int GetRequiredCell(
        WorkoutImportProperty property)
    {
        if (TryGetColumn(
                property,
                out var cellIndex))
        {
            return cellIndex;
        }

        throw new InvalidOperationException(
            $"No Excel column mapping exists for workout import property " +
            $"'{property}'.");
    }
}


public sealed class ExcelWorkoutImportColumnMapping
{
    public int CellIndex { get; init; }

    public WorkoutImportProperty Property { get; init; }
}

public enum WorkoutImportProperty
{
    Date,
    Order,
    Exercise,
    Details,
    Notes
}