using PRLab.API.DTO.Workout.WorkoutBlock;
using PRLab.API.DTO.Workout.WorkoutSegments;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.WorkoutStructure;
using PRLab.Domain.Utilities;

namespace PRLab.API.Mapper.WorkoutMappers;

public static class WorkoutBlockMapper
{
    public static IReadOnlyCollection<WorkoutBlockGetDTO> ToGetDTOs(
        IReadOnlyCollection<WorkoutBlock> workoutBlocks)
    {
        return ToGetDTOs(
            workoutBlocks,
            (LocalizationHelper.Language?)null);
    }

    public static IReadOnlyCollection<WorkoutBlockGetDTO> ToGetDTOs(
        IReadOnlyCollection<WorkoutBlock> workoutBlocks,
        LocalizationHelper.Language? language)
    {
        ArgumentNullException.ThrowIfNull(workoutBlocks);

        return workoutBlocks
            .Select(workoutBlock => ToGetDTO(workoutBlock, language))
            .ToList();
    }

    public static WorkoutBlockGetDTO ToGetDTO(
        WorkoutBlock workoutBlock)
    {
        return ToGetDTO(
            workoutBlock,
            (LocalizationHelper.Language?)null);
    }
    
    public static WorkoutBlockGetDTO ToGetDTO(
        WorkoutBlock workoutBlock,
        LocalizationHelper.Language? language)
    {
        ArgumentNullException.ThrowIfNull(workoutBlock);

        return new WorkoutBlockGetDTO(
            workoutBlock.Id,
            workoutBlock.Name,
            workoutBlock.BlockType,
            workoutBlock.BlockRepeatPrescription,
            workoutBlock.Segments
                .OrderBy(segment => segment.Sequence)
                .Select(segment =>
                    WorkoutBlockSegmentMapper.ToGetDTO(
                        segment,
                        language))
                .ToList(),
            workoutBlock.Visibility.Scope);
    }
    
    public static WorkoutBlock ToEntity(
        WorkoutBlockPostDTO payload,
        User user)
    {
        ArgumentNullException.ThrowIfNull(payload);
        ArgumentNullException.ThrowIfNull(user);

        ValidateSegmentSequences(payload.Segments);

        var workoutBlock = WorkoutBlock.NewUserCreated(
            name: payload.Name,
            blockType: payload.BlockType,
            owner: user,
            repeatPrescription: payload.RepeatPrescription,
            visibilityScope: payload.Visibility);

        foreach (var segmentPayload in payload.Segments
                     .OrderBy(segment => segment.Sequence))
        {
            var segment = WorkoutBlockSegmentMapper.ToEntity(
                segmentPayload,
                workoutBlock.Id);

            workoutBlock.AddSegment(
                segment,
                user,
                segmentPayload.Sequence);
        }

        return workoutBlock;
    }

     public static WorkoutBlockAssignment ToAssignment(WorkoutId workoutId, WorkoutBlock workoutBlock, int? sequence)
    {
       return WorkoutBlockAssignment. New(workoutId, workoutBlock, sequence);
    }
    
    private static void ValidateSegmentSequences(
        IReadOnlyCollection<WorkoutBlockSegmentPostDTO> segments)
    {
        ArgumentNullException.ThrowIfNull(segments);

        var invalidSequence = segments
            .FirstOrDefault(segment => segment.Sequence < 1);

        if (invalidSequence is not null)
        {
            throw new ArgumentException(
                "Workout block segment sequences must be greater than zero.",
                nameof(segments));
        }

        var duplicateSequences = segments
            .GroupBy(segment => segment.Sequence)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .OrderBy(sequence => sequence)
            .ToList();

        if (duplicateSequences.Count > 0)
        {
            throw new ArgumentException(
                $"Workout block segment sequences contain duplicate value(s): " +
                $"{string.Join(", ", duplicateSequences)}.",
                nameof(segments));
        }
    }
}