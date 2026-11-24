using PRLab.Domain.Model.Value.Enum.Workout;

namespace PRLab.Application.WorkoutImport.Model;

public sealed class WorkoutImportProfile
{
    public required string Name { get; init; }

    public IReadOnlyList<WorkoutImportSourceProfile> SourceProfiles { get; init; } = [];

    public IReadOnlyDictionary<string, IReadOnlyList<string>> ExerciseAliases { get; init; }
        = new Dictionary<string, IReadOnlyList<string>>();

    public IReadOnlyDictionary<string, WorkoutBlockType> BlockMappings { get; init; }
        = new Dictionary<string, WorkoutBlockType>();

    public IReadOnlyList<string> IgnoredLabels { get; init; } = [];

    public IReadOnlyList<string> Separators { get; init; } = [];
    
    public string? InterpretationInstructions { get; init; } //todo would belong in llm sourceproile

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