using PRLab.API.DTO.Muscle;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Update;

namespace PRLab.API.Mapper.UpdateMapper;

public static class MuscleUpdateMapper
{
    public static MuscleUpdate ToUpdate(
        MusclePutDTO payload,
        User? currentUser)
    {
        ArgumentNullException.ThrowIfNull(payload);

        return new MuscleUpdate
        {
            Name = payload.Name,
            LatinName = payload.LatinName,
            LatinNameWasProvided = true,
            BodySection = payload.BodySection,
            DescriptionUpdate = DescriptionUpdateMapper.ToUpdate(payload.Description),
            UpdatedBy = currentUser,
        };
    }
}