using PRLab.API.DTO.Muscle;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities;

namespace PRLab.API.Mapper;

public static class MuscleMapper
{
    public static MuscleGetDTO ToGetDTO(Muscle muscle)
    {
        return ToGetDTO(
            muscle,
            (LocalizationHelper.Language?)null
        );
    }

    public static MuscleGetDTO ToGetDTO(
        Muscle muscle,
        LocalizationHelper.Language? language)
    {
        ArgumentNullException.ThrowIfNull(muscle);

        return new MuscleGetDTO(
                muscle.Id,
                muscle.Name,
                muscle.LatinName,
                muscle.BodySection,
                DescriptionMapper.ToGetDTO(muscle.Description, language),
                muscle.Antagonists
                    .Where(antagonist => antagonist.AntagonistMuscle is not null)
                    .Select(antagonist => ToSummaryDTO(antagonist.AntagonistMuscle))
                    .ToList()
            );
        
    }

    public static IReadOnlyCollection<MuscleGetDTO> ToGetDTOs(
        IReadOnlyCollection<Muscle> muscles)
    {
        return ToGetDTOs(
            muscles,
            (LocalizationHelper.Language?)null
        );
    }

    public static IReadOnlyCollection<MuscleGetDTO> ToGetDTOs(
        IReadOnlyCollection<Muscle> muscles,
        LocalizationHelper.Language? language)
    {
        ArgumentNullException.ThrowIfNull(muscles);

        return muscles
            .Select(muscle => ToGetDTO(muscle, language))
            .ToList();
    }

    public static MuscleSummaryDTO ToSummaryDTO(Muscle muscle)
    {
        ArgumentNullException.ThrowIfNull(muscle);

        return new MuscleSummaryDTO(
            muscle.Id,
            muscle.Name,
            muscle.LatinName,
            muscle.BodySection
        );
    }

    public static IReadOnlyCollection<MuscleSummaryDTO> ToSummaryDTOs(
        IReadOnlyCollection<Muscle> muscles)
    {
        ArgumentNullException.ThrowIfNull(muscles);

        return muscles
            .Select(ToSummaryDTO)
            .ToList();
    }

    public static Muscle ToEntity(MusclePostDTO payload, User? currentUser)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var description = payload.Descriptor is null
            ? Description.New(null)
            : DescriptionMapper.ToEntity(payload.Descriptor);

        var muscle = Muscle.New(
            payload.Name,
            payload.LatinName,
            payload.BodySection,
            description,
            currentUser
        );

        if (payload.AntagonistIds is null)
        {
            return muscle;
        }

        foreach (var antagonistId in payload.AntagonistIds.Distinct())
        {
            muscle.AddAntagonist(
                antagonistId,
                currentUser
            );
        }

        return muscle;
    }
}