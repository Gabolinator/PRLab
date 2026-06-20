using Microsoft.Extensions.Logging;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding.Factory.Entity.Workout;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Model.Value.Enum.Prescription.Load;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Enum.Workout;
using PRLab.Domain.Model.Value.Prescription.Common;
using PRLab.Domain.Model.Value.Prescription.Load;
using PRLab.Domain.Model.Value.Prescription.Rest;
using PRLab.Domain.Model.Value.Prescription.Time;
using PRLab.Domain.Model.Value.Prescription.Work;
using PRLab.Domain.Model.Value.Prescription.Workout;
using PRLab.Domain.Model.Value.WorkoutValue;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.DB.Helpers;

namespace PRLab.Infrastructure.DB.Seeding.Development.Factory.WorkoutFactory;

public class DevelopmentWorkoutSeedFactory(
    IUserService userService,
    IAppLogger logger) : IWorkoutSeedFactory
{
    private User SeedUser => userService.GetSystemAdminUser("Seed");

    public IReadOnlyList<SeedItem<Workout>> CreateInitialData(ExerciseSeedCatalog catalog)
    {
        var run = catalog.GetRequiredByNameKey("running");
        var burpeePullUp = catalog.GetRequiredByNameKey("burpeepullup");
        var wallBall = catalog.GetRequiredByNameKey("wallball");
        var deadlift = catalog.GetRequiredByNameKey("deadlift");
        var row = catalog.GetRequiredByNameKey("row");

        var assaultBike = catalog.GetRequiredByNameKey("assaultbike");
        var skiErg = catalog.GetRequiredByNameKey("skierg");
        var lateralBurpeeOverBarbell = catalog.GetRequiredByNameKey("lateralburpeeoverbarbell");
        var backSquat = catalog.GetRequiredByNameKey("backsquat");
        var doubleDumbbellHangPowerClean = catalog.GetRequiredByNameKey("doubledumbbellhangpowerclean");

        var workouts = new List<Workout>
        {
            CreateMixedConditioningWorkout(
                run,
                burpeePullUp,
                wallBall,
                deadlift,
                row),

            CreateHyroxWorkout(
                assaultBike,
                skiErg,
                lateralBurpeeOverBarbell),

            CreateCrossFitWorkout(
                assaultBike,
                backSquat,
                doubleDumbbellHangPowerClean)
        };

        return workouts
            .Select(workout => new SeedItem<Workout>(
                SeedKeyGenerator.GenerateWorkoutKey(workout),
                workout,
                SeedAction.CreateIfMissing))
            .ToList();
    }

    private static Workout CreateMixedConditioningWorkout(
        Exercise run,
        Exercise burpeePullUp,
        Exercise wallBall,
        Exercise deadlift,
        Exercise row)
    {
        var workout = Workout.NewBuiltIn(
            name: "Mixed Conditioning + Deadlift",
            description: "3 repeats of mixed conditioning segments, then rest before block 2.");

        var block1 = WorkoutBlock.NewBuiltIn(
            name: "Block 1",
            blockType: WorkoutBlockType.Metcon,
            repeatPrescription: BlockRepeatPrescription.Repeat(
                repeatCount: 3,
                restAfterBlock: RestTarget.SecondsDuration(300)));

        var segment1 = WorkoutBlockSegment.ForTime(
            workoutBlockId: block1.Id,
            name: "Run + Burpee Pull-Up",
            sequence: 1,
            targetIntensity: null,
            estimatedDuration: EstimatedDuration.Minutes(3),
            restAfterSegment: RestTarget.SecondsDuration(120));

        segment1.AddStep(
            WorkoutBlockSegmentStep.NewExerciseStep(
                segmentId: segment1.Id,
                exerciseId: run.Id,
                sequence: 1,
                prescription: WorkoutStepPrescription.New(
                    workTarget: WorkTarget.ForDistance(400),
                    timeConstraint: TimeConstraint.Cap(TimeSpan.FromMinutes(2)))));

        segment1.AddStep(
            WorkoutBlockSegmentStep.NewExerciseStep(
                segmentId: segment1.Id,
                exerciseId: burpeePullUp.Id,
                sequence: 2,
                prescription: WorkoutStepPrescription.New(
                    workTarget: WorkTarget.ForReps(10))));

        block1.AddSegment(segment1);

        var segment2 = WorkoutBlockSegment.ForTime(
            workoutBlockId: block1.Id,
            name: "Wall Ball + Row",
            sequence: 2,
            targetIntensity: null,
            restAfterSegment: RestTarget.SecondsDuration(120));

        segment2.AddStep(
            WorkoutBlockSegmentStep.NewExerciseStep(
                segmentId: segment2.Id,
                exerciseId: wallBall.Id,
                sequence: 1,
                prescription: WorkoutStepPrescription.New(
                    workTarget: WorkTarget.ForReps(30),
                    loadTarget: LoadTarget.ExternalLoad(9, LoadUnit.Kilogram),
                    estimatedDuration: EstimatedDuration.Seconds(100),
                    timeConstraint: TimeConstraint.Cap(TimeSpan.FromMinutes(2)))));

        segment2.AddStep(
            WorkoutBlockSegmentStep.NewExerciseStep(
                segmentId: segment2.Id,
                exerciseId: row.Id,
                sequence: 2,
                prescription: WorkoutStepPrescription.New(
                        workTarget: WorkTarget.ForCalories(20))
                    .WithEstimatedDurationSeconds(60)));

        block1.AddSegment(segment2);

        var segment3 = WorkoutBlockSegment.FixedWork(
            workoutBlockId: block1.Id,
            name: "Deadlift 2RM",
            sequence: 3,
            intent: WorkIntent.MaxEffort,
            scoreType: WorkoutScoreType.MaxLoad);

        segment3.AddStep(
            WorkoutBlockSegmentStep.NewExerciseStep(
                segmentId: segment3.Id,
                exerciseId: deadlift.Id,
                sequence: 1,
                prescription: WorkoutStepPrescription.New(
                    workTarget: WorkTarget.ForReps(2),
                    partition: WorkPartitionPrescription.Repeated(
                        repeatCount: 3,
                        restBetweenRepeats: RestTarget.SecondsDuration(180)),
                    loadTarget: LoadTarget.RepMax(2),
                    notes: "Build to 2RM.")));

        block1.AddSegment(segment3);

        workout.AddBlock(
            WorkoutBlockAssignment.New(
                workoutId: workout.Id,
                workoutBlock: block1,
                sequence: 1));

        return workout;
    }

    private static Workout CreateHyroxWorkout(
        Exercise assaultBike,
        Exercise skiErg,
        Exercise lateralBurpeeOverBarbell)
    {
        var workout = Workout.NewBuiltIn(
            name: "Ausdauer / Hyrox",
            description: "7 repeats of rotating intervals: bike, ski, lateral burpees, rest, rest.");

        var block = WorkoutBlock.NewBuiltIn(
            name: "Ausdauer",
            blockType: WorkoutBlockType.Endurance,
            repeatPrescription: BlockRepeatPrescription.Repeat(
                repeatCount: 7,
                estimatedRepeatDuration: EstimatedDuration.Minutes(7.5f)));

        var segment = WorkoutBlockSegment.StepIntervals(
            workoutBlockId: block.Id,
            name: "Every 1:30 Rotation",
            sequence: 1,
            intent: WorkIntent.Normal,
            targetIntensity: null,
            stepIntervalSeconds: 90,
            scoreType: WorkoutScoreType.Completed);

        segment.AddLast(
            WorkoutBlockSegmentStep.NewExerciseStep(
                segmentId: segment.Id,
                exerciseId: assaultBike.Id,
                sequence: 1,
                prescription: WorkoutStepPrescription.New(
                    workTarget: WorkTarget.ForCalories(20),
                    notes: "Scaling: 20/15, 15/11, 11/8 calories.")));

        segment.AddLast(
            WorkoutBlockSegmentStep.NewExerciseStep(
                segmentId: segment.Id,
                exerciseId: skiErg.Id,
                sequence: 2,
                prescription: WorkoutStepPrescription.New(
                    workTarget: WorkTarget.ForCalories(20),
                    notes: "Scaling: 20/15, 15/11, 11/8 calories.")));

        segment.AddLast(
            WorkoutBlockSegmentStep.NewExerciseStep(
                segmentId: segment.Id,
                exerciseId: lateralBurpeeOverBarbell.Id,
                sequence: 3,
                prescription: WorkoutStepPrescription.New(
                    workTarget: WorkTarget.ForReps(18),
                    notes: "Scaling: 18, 14, 11 reps.")));

        segment.AddLast(
            WorkoutBlockSegmentStep.NewRestStep(
                segmentId: segment.Id,
                rest: RestTarget.SecondsDuration(90),
                sequence: 4));

        segment.AddLast(
            WorkoutBlockSegmentStep.NewRestStep(
                segmentId: segment.Id,
                rest: RestTarget.SecondsDuration(90),
                sequence: 5));

        block.AddSegment(segment);

        workout.AddBlock(
            WorkoutBlockAssignment.New(
                workoutId: workout.Id,
                workoutBlock: block,
                sequence: 1));

        return workout;
    }

    private static Workout CreateCrossFitWorkout(
        Exercise assaultBike,
        Exercise backSquat,
        Exercise doubleDumbbellHangPowerClean)
    {
        var workout = Workout.NewBuiltIn(
            name: "CrossFit Back Squat + Metcon",
            description: "Back squat strength piece followed by 5-repeat metcon.");

        var strengthBlock = WorkoutBlock.NewBuiltIn(
            name: "Back Squat",
            blockType: WorkoutBlockType.Strength,
            repeatPrescription: BlockRepeatPrescription.Repeat(
                repeatCount: 10,
                restBetweenRepeats: RestTarget.SecondsDuration(120),
                estimatedRepeatDuration: EstimatedDuration.Minutes(2)));

        var strengthSegment = WorkoutBlockSegment.FixedWork(
            workoutBlockId: strengthBlock.Id,
            name: "Back Squat 1x1",
            sequence: 1,
            intent: WorkIntent.MaxEffort,
            scoreType: WorkoutScoreType.MaxLoad,
            estimatedDuration: EstimatedDuration.Minutes(20));

        strengthSegment.AddLast(
            WorkoutBlockSegmentStep.NewExerciseStep(
                segmentId: strengthSegment.Id,
                exerciseId: backSquat.Id,
                sequence: 1,
                prescription: WorkoutStepPrescription.New(
                    workTarget: WorkTarget.ForReps(1),
                    loadTarget: LoadTarget.PercentageRepMax(70),
                    notes: "Go every 2 minutes.")));

        strengthBlock.AddSegment(strengthSegment);

        var metconBlock = WorkoutBlock.NewBuiltIn(
            name: "Metcon",
            blockType: WorkoutBlockType.Metcon,
            repeatPrescription: BlockRepeatPrescription.Repeat(
                repeatCount: 5,
                estimatedRepeatDuration: EstimatedDuration.Minutes(4)));

        var metconSegment = WorkoutBlockSegment.ForTimeWithCap(
            workoutBlockId: metconBlock.Id,
            name: "Bike + Dumbbell Hang Power Cleans",
            sequence: 1,
            cap: TimeSpan.FromMinutes(2),
            targetIntensity: null,
            restAfterSegment: RestTarget.SecondsDuration(120));

        metconSegment.AddLast(
            WorkoutBlockSegmentStep.NewExerciseStep(
                segmentId: metconSegment.Id,
                exerciseId: assaultBike.Id,
                sequence: 1,
                prescription: WorkoutStepPrescription.New(
                    workTarget: WorkTarget.ForCalories(20),
                    notes: "Scaling: 20/15, 15/11, 11/8 calories.")));

        metconSegment.AddLast(
            WorkoutBlockSegmentStep.NewExerciseStep(
                segmentId: metconSegment.Id,
                exerciseId: doubleDumbbellHangPowerClean.Id,
                sequence: 2,
                prescription: WorkoutStepPrescription.New(
                    workTarget: WorkTarget.ForReps(17),
                    loadTarget: LoadTarget.ExternalLoad(22.5m, LoadUnit.Kilogram),
                    notes: "Scaling: 22.5/15, 15/10, 10/7.5 kg.")));

        metconBlock.AddSegment(metconSegment);

        workout.AddBlock(
            WorkoutBlockAssignment.New(
                workoutId: workout.Id,
                workoutBlock: strengthBlock,
                sequence: 1));

        workout.AddBlock(
            WorkoutBlockAssignment.New(
                workoutId: workout.Id,
                workoutBlock: metconBlock,
                sequence: 2));

        return workout;
    }
}