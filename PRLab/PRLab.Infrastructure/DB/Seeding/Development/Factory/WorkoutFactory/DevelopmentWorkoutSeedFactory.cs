using Microsoft.Extensions.Logging;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding.Factory.Entity.Workout;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Application.Models.DB.Seeding.Catalog;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Model.Value.Enum.Prescription;
using PRLab.Domain.Model.Value.Enum.Workout;
using PRLab.Domain.Model.Value.Prescription;
using PRLab.Domain.Model.Value.WorkoutValue;
using PRLab.Infrastructure.DB.Seeding.Development.Factory.MovementFactory;

namespace PRLab.Infrastructure.DB.Seeding.Development.Factory.WorkoutFactory;

public class DevelopmentWorkoutSeedFactory(
    IUserService userService,
    ExerciseSeedCatalog catalog,
    ILogger<DevelopmentMovementSeedFactory> logger) : IWorkoutSeedFactory
{
    private User SeedUser => userService.GetSystemAdminUser("Seed");

    public IReadOnlyList<SeedItem<Workout>> CreateInitialData()
    {
        var run = catalog.GetRequiredByName("running");
        var burpeePullUp = catalog.GetRequiredByName("burpeepullup");
        var wallBall = catalog.GetRequiredByName("wallball");
        var deadlift = catalog.GetRequiredByName("deadlift");
        var row = catalog.GetRequiredByName("row");

        var workout = Workout.NewBuiltIn(
            name: "Mixed Conditioning + Deadlift",
            description: "3 rounds of mixed conditioning segments, then rest before block 2.");

        // Block 1:
        // Do all segments 3 times.
        // Rest 5 min after the whole block.
        var block1 = WorkoutBlock.NewBuiltIn(
            name: "Block 1",
            blockType: WorkoutBlockType.Metcon,
            roundPrescription: RoundPrescription.Rounds(
                numRounds: 3,
                restAfterBlock: RestTarget.SecondsDuration(300)));

        // Segment 1:
        // Run 400m with 2 min cap,
        // then 10 burpee pull-ups for time,
        // then rest 2 min.
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
                exercise: run, // error it was written deadlift
                sequence: 1,
                prescription: WorkoutStepPrescription.New(
                    workTarget: WorkTarget.ForDistance(400),
                    timeConstraint: TimeConstraint.Cap(TimeSpan.FromMinutes(2)))));

        segment1.AddStep(
            WorkoutBlockSegmentStep.NewExerciseStep(
                segmentId: segment1.Id,
                exercise: burpeePullUp,
                sequence: 2,
                prescription: WorkoutStepPrescription.New(
                    workTarget: WorkTarget.ForReps(10))));

        block1.AddSegment(segment1);

        // Segment 2:
        // 30 wall balls,
        // then 20 cal row,
        // then rest 2 min.
        var segment2 = WorkoutBlockSegment.ForTime(
            workoutBlockId: block1.Id,
            name: "Wall Ball + Row",
            sequence: 2,
            targetIntensity: null,
            restAfterSegment: RestTarget.SecondsDuration(120));

        segment2.AddStep(
            WorkoutBlockSegmentStep.NewExerciseStep(
                segmentId: segment2.Id,
                exercise: wallBall,
                sequence: 1,
                prescription: WorkoutStepPrescription.New(
                    workTarget: WorkTarget.ForReps(30),
                    loadTarget: LoadTarget.ExternalLoad(9, LoadUnit.Kilogram),
                    estimatedDuration: EstimatedDuration.Seconds(100),
                    timeConstraint: TimeConstraint.Cap(TimeSpan.FromMinutes(2)))));

        segment2.AddStep(
            WorkoutBlockSegmentStep.NewExerciseStep(
                segmentId: segment2.Id,
                exercise: row,
                sequence: 2,
                prescription: WorkoutStepPrescription.New(
                        workTarget: WorkTarget.ForCalories(20))
                    .WithEstimatedDurationSeconds(60)));

        block1.AddSegment(segment2);

        // Segment 3:
        // 3 sets of 2RM deadlift,
        // 3 min rest after each step/set.
        var segment3 = WorkoutBlockSegment.FixedWork(
            workoutBlockId: block1.Id,
            name: "Deadlift 2RM",
            sequence: 3,
            intent: WorkIntent.MaxEffort,
            scoreType: WorkoutScoreType.MaxLoad);

        segment3.AddStep(
            WorkoutBlockSegmentStep.NewExerciseStep(
                segmentId: segment3.Id,
                exercise: deadlift,
                sequence: 1,
                prescription: WorkoutStepPrescription.New(
                    workTarget: WorkTarget.ForReps(2),
                    sets: 3,
                    loadTarget: LoadTarget.RepMax(2),
                    notes: "Build to 2RM.")));

        block1.AddSegment(segment3);

        // Attach block to workout.
        var block1Assignment = WorkoutBlockAssignment.New(
            workoutId: workout.Id,
            workoutBlockId: block1.Id,
            sequence: 1);

        workout.AddBlock(block1Assignment);


        //todo



        // Existing Exercise objects assumed:
// assaultBike
// skiErg
// lateralBurpeeOverBarbell
// backSquat
// doubleDumbbellHangPowerClean

        var hyroxWorkout = Workout.NewBuiltIn(
            name: "Ausdauer / Hyrox",
            description: "7 rounds of rotating intervals: bike, ski, lateral burpees, rest, rest.");

        var hyroxBlock = WorkoutBlock.NewBuiltIn(
            name: "Ausdauer",
            blockType: WorkoutBlockType.Endurance,
            roundPrescription: RoundPrescription.Rounds(
                numRounds: 7,
                estimatedDuration: EstimatedDuration.Minutes(7.5f)));

        var hyroxSegment = WorkoutBlockSegment.StepIntervals(
            workoutBlockId: hyroxBlock.Id,
            name: "Every 1:30 Rotation",
            sequence: 1,
            intent: WorkIntent.Normal,
            targetIntensity: null,
            stepIntervalSeconds: 90,
            scoreType: WorkoutScoreType.Completed);

        var assaultBike = catalog.GetRequiredByName("assaultbike");
        var skiErg = catalog.GetRequiredByName("skierg");
        var lateralBurpeeOverBarbell = catalog.GetRequiredByName("lateralBurpeeOverBarbell");
        var backSquat = catalog.GetRequiredByName("backsquat");
        var doubleDumbbellHangPowerClean = catalog.GetRequiredByName("doubleDumbbellHangPowerClean");


        hyroxSegment.AddLast(
            WorkoutBlockSegmentStep.NewExerciseStep(
                segmentId: hyroxSegment.Id,
                exercise: assaultBike,
                sequence: 1,
                prescription: WorkoutStepPrescription.New(
                    workTarget: WorkTarget.ForCalories(20),
                    notes: "Scaling: 20/15, 15/11, 11/8 calories.")));

        hyroxSegment.AddLast(
            WorkoutBlockSegmentStep.NewExerciseStep(
                segmentId: hyroxSegment.Id,
                exercise: skiErg,
                sequence: 1,
                prescription: WorkoutStepPrescription.New(
                    workTarget: WorkTarget.ForCalories(20),
                    notes: "Scaling: 20/15, 15/11, 11/8 calories.")));

        hyroxSegment.AddLast(
            WorkoutBlockSegmentStep.NewExerciseStep(
                segmentId: hyroxSegment.Id,
                exercise: lateralBurpeeOverBarbell,
                sequence: 1,
                prescription: WorkoutStepPrescription.New(
                    workTarget: WorkTarget.ForReps(18),
                    notes: "Scaling: 18, 14, 11 reps.")));

        hyroxSegment.AddLast(
            WorkoutBlockSegmentStep.NewRestStep(
                segmentId: hyroxSegment.Id,
                rest: RestTarget.SecondsDuration(90),
                sequence: 1));

        hyroxSegment.AddLast(
            WorkoutBlockSegmentStep.NewRestStep(
                segmentId: hyroxSegment.Id,
                rest: RestTarget.SecondsDuration(90),
                sequence: 1));

        hyroxBlock.AddSegment(hyroxSegment);

        hyroxWorkout.AddBlock(
            WorkoutBlockAssignment.New(
                workoutId: hyroxWorkout.Id,
                workoutBlock: hyroxBlock,
                sequence: 1));

        var crossFitWorkout = Workout.NewBuiltIn(
            name: "CrossFit Back Squat + Metcon",
            description: "Back squat strength piece followed by 5-round metcon.");

        var strengthBlock = WorkoutBlock.NewBuiltIn(
            name: "Back Squat",
            blockType: WorkoutBlockType.Strength,
            roundPrescription: RoundPrescription.Rounds(
                numRounds: 10,
                restBetweenRounds: RestTarget.SecondsDuration(120),
                estimatedDuration: EstimatedDuration.Minutes(2)));

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
                exercise: backSquat,
                sequence: 1,
                prescription: WorkoutStepPrescription.New(
                    workTarget: WorkTarget.ForReps(1),
                    sets: 1,
                    loadTarget: LoadTarget.PercentageRepMax(70),
                    notes: "Go every 2 minutes.")));

        strengthBlock.AddSegment(strengthSegment);

        var metconBlock = WorkoutBlock.NewBuiltIn(
            name: "Metcon",
            blockType: WorkoutBlockType.Metcon,
            roundPrescription: RoundPrescription.Rounds(
                numRounds: 5,
                estimatedDuration: EstimatedDuration.Minutes(4)));

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
                exercise: assaultBike,
                sequence: 1,
                prescription: WorkoutStepPrescription.New(
                    workTarget: WorkTarget.ForCalories(20),
                    notes: "Scaling: 20/15, 15/11, 11/8 calories.")));

        metconSegment.AddLast(
            WorkoutBlockSegmentStep.NewExerciseStep(
                segmentId: metconSegment.Id,
                exercise: doubleDumbbellHangPowerClean,
                sequence: 1,
                prescription: WorkoutStepPrescription.New(
                    workTarget: WorkTarget.ForReps(17),
                    loadTarget: LoadTarget.ExternalLoad(22.5m, LoadUnit.Kilogram),
                    notes: "Scaling: 22.5/15, 15/10, 10/7.5 kg.")));

        metconBlock.AddSegment(metconSegment);

        crossFitWorkout.AddBlock(
            WorkoutBlockAssignment.New(
                workoutId: crossFitWorkout.Id,
                workoutBlock: strengthBlock,
                sequence: 1));

        crossFitWorkout.AddBlock(
            WorkoutBlockAssignment.New(
                workoutId: crossFitWorkout.Id,
                workoutBlock: metconBlock,
                sequence: 2));

        var workouts = new List<Workout>{workout, hyroxWorkout , crossFitWorkout};

        return workouts.Select(workout=> new SeedItem<Workout>(
            SeedKeyGenerator.GenerateWorkoutKey(workout),
            workout,
            SeedAction.CreateIfMissing)).ToList();
    }


}