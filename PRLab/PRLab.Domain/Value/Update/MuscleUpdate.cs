using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities;

namespace PRLab.Domain.Value.Update;

public class MuscleUpdate
{
    public string? Name { get; init; }

    public string? LatinName { get; init; }

    public bool LatinNameWasProvided { get; init; }
    
    public DescriptionUpdate? DescriptionUpdate { get; init; }
    
    public DomainEnum.BodySection? BodySection { get; init; }
    
    public User? UpdatedBy { get; init; }

    public static MuscleUpdate FromMuscle(
        Muscle muscle,
        LocalizationHelper.Language? language,
        User? user)
    {
        return new MuscleUpdate
        {
            Name = muscle.Name,
            BodySection = muscle.BodySection,
            LatinName = muscle.LatinName,
            LatinNameWasProvided = !string.IsNullOrWhiteSpace(muscle.LatinName),
            DescriptionUpdate = DescriptionUpdate.FromDescription(muscle.Description, language, user),
            UpdatedBy = user
        };
    }
}