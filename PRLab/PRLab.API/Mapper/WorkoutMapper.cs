using PRLab.API.DTO.Workout;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities;

namespace PRLab.API.Mapper;

public static class WorkoutMapper
{
    public static IReadOnlyCollection<WorkoutGetDTO> ToGetDTOs(
        IReadOnlyCollection<Workout> workouts,
        LocalizationHelper.Language? language)
    {
        ArgumentNullException.ThrowIfNull(workouts);

        return workouts
            .Select(movement => ToGetDTO(movement, language))
            .ToList();
    }
    
    public static WorkoutGetDTO ToGetDTO(Workout workout)
    {
        return ToGetDTO(
            workout,
            (LocalizationHelper.Language?)null);
    }

    public static WorkoutGetDTO ToGetDTO(
        Workout workout,
        LocalizationHelper.Language? language)
    {
        return new WorkoutGetDTO
        {

        };
    }       

    public static Workout ToEntity(WorkoutPostDTO payload, User user)
    {
        //todo
       throw new NotImplementedException();                                         
    }
}