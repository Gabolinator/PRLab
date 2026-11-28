using Microsoft.Extensions.Options;
using PRLab.Application.Models.ImportDraft;
using PRLab.Application.WorkoutImport.Config;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Enum.Workout;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities;
using PRLab.Infrastructure.Utilities;
using PRLab.Infrastructure.WorkoutImport;
using PRLab.Infrastructure.WorkoutImport.Extractor.Excel;
using PRLab.Infrastructure.WorkoutImport.Interpreter;

namespace PRLab.Tests.InfrastructureTests;

public sealed class CoachExcelWorkoutImportTests
{
    [Fact]
    public async Task ImportsStickWithoutCatalogEquipmentAndLeavesNotesUnconsumed()
    {
        const string exercise = "Lying On Belly On Floor Snatch Grip Overhead Hold";
        const string notes = "could not do it unbroken. had to stop at 90s take a few sec pause then continue";
        var draft = await ImportAsync(exercise, "stick | 1x120s", notes, [Reference(exercise, WorkTargetType.DurationSeconds)]);
        var row = Assert.Single(draft.Rows);
        var items = Assert.Single(row.ExerciseInterpretations).SemanticItems;

        Assert.Equal("Stick", Assert.Single(items.OfType<WorkoutEquipmentSemantic>()).Name);
        Assert.Equal(120m, Assert.Single(items.OfType<WorkoutWorkSemantic>()).Value);
        Assert.Contains(row.UnconsumedText, fragment => fragment.Text == notes);
    }

    [Fact]
    public async Task PreservesBackSquatCadenceAlongsideSetupEquipment()
    {
        var squat = Reference("Back Squat", WorkTargetType.Repetitions) with { SupportsExternalLoad = true };
        var draft = await ImportAsync("back squat", "on medball | belt | 16x1x90 0:50", null, [squat]);
        var row = Assert.Single(draft.Rows);
        var equipment = Assert.Single(row.ExerciseInterpretations).SemanticItems.OfType<WorkoutEquipmentSemantic>();

        Assert.Contains(equipment, item => item is { Name: "Medicine Ball", Relation: WorkoutEquipmentRelation.On });
        Assert.Contains(equipment, item => item.Name == "Weightlifting Belt");
        var step = Assert.Single(row.Prescriptions).Step;
        Assert.Equal(16, step.SetCount);
        Assert.Equal(1m, step.WorkValue);
        Assert.Equal(90m, step.LoadValue);
        Assert.Equal(50, step.SetCadenceSeconds);
    }

    [Fact]
    public async Task ImportsSpeedSquatCuesAndPreservesTheOrderedLoadSeries()
    {
        const string details = "max speed up | paused | oly shoes | on med ball | 1x20-40-60-70-80 eigene zeit";
        var draft = await ImportAsync("speed squat", details, null, [Reference("Squat", WorkTargetType.Repetitions)]);
        var row = Assert.Single(draft.Rows);
        var items = Assert.Single(row.ExerciseInterpretations).SemanticItems;

        Assert.Equal("Speed", Assert.Single(items.OfType<WorkoutMovementQualitySemantic>()).Quality);
        Assert.Null(Assert.Single(items.OfType<WorkoutPauseSemantic>()).Position);
        Assert.Single(items.OfType<WorkoutSelfPacedSemantic>());
        Assert.Contains(items.OfType<WorkoutEquipmentSemantic>(), item => item.Name == "Weightlifting Shoes");
        Assert.Contains(items.OfType<WorkoutEquipmentSemantic>(), item =>
            item is { Name: "Medicine Ball", Relation: WorkoutEquipmentRelation.On });
        var series = row.Prescriptions.Where(prescription => prescription.Step.LoadValue.HasValue).ToList();
        Assert.Equal(new decimal?[] { 20, 40, 60, 70, 80 }, series.Select(prescription => prescription.Step.LoadValue));
        Assert.All(series, prescription => Assert.True(prescription.Step is { SetCount: 1, WorkValue: 1 }));
        Assert.DoesNotContain(row.UnconsumedText, fragment => fragment.Text.Contains("max speed up", StringComparison.Ordinal));
    }

    [Fact]
    public async Task AssociatesHoldWithRepsPerSideAndPreservesExactSpans()
    {
        const string source = "self supported dragon squat elbow on box 5x each side 5s hold";
        var draft = await ImportAsync(source, null, null, [Reference("Squat", WorkTargetType.Repetitions)]);
        var row = Assert.Single(draft.Rows);
        var items = Assert.Single(row.ExerciseInterpretations).SemanticItems;
        var work = Assert.Single(items.OfType<WorkoutWorkSemantic>());
        var hold = Assert.Single(items.OfType<WorkoutHoldSemantic>());

        Assert.Equal("5x each side", work.Origin.Source.Text);
        Assert.Equal(source.IndexOf("5x each side", StringComparison.Ordinal), work.Origin.Source.Offset);
        Assert.Equal("5s hold", hold.Origin.Source.Text);
        Assert.Equal(source.IndexOf("5s hold", StringComparison.Ordinal), hold.Origin.Source.Offset);
        Assert.Equal(work.Origin, hold.WorkOrigin);
        var step = Assert.Single(row.Prescriptions).Step;
        Assert.Equal(5m, step.WorkValue);
        Assert.Equal(WorkTargetScope.PerSide, step.Scope);
        Assert.Equal(5, step.HoldSecondsPerRep);
        Assert.Contains(row.UnconsumedText, fragment => fragment.Text.Trim() == "elbow on box");
    }

    [Fact]
    public async Task UsesBoxMetadataForHeightAndPreservesZeroLoad()
    {
        const string exercise = "Deep Lateral Box Step Up";
        const string details = "61cm | 1db | 16x0 each side, own time";
        var boxStep = Reference(exercise, WorkTargetType.Repetitions) with { EquipmentNames = ["Plyometric Box"] };
        var draft = await ImportAsync(exercise, details, null, [boxStep]);
        var row = Assert.Single(draft.Rows);
        var equipment = Assert.Single(row.ExerciseInterpretations).SemanticItems.OfType<WorkoutEquipmentSemantic>();

        Assert.Contains(equipment, item => item is { Name: "Plyometric Box", HeightCentimeters: 61 });
        Assert.Contains(equipment, item => item is { Name: "Dumbbell", Quantity: 1 });
        Assert.Contains(row.Prescriptions, entry => entry.Step is { WorkValue: 16, Scope: WorkTargetScope.PerSide, LoadValue: 0 });
        Assert.Contains(row.Prescriptions, entry => entry.Step.SelfPaced);

        var withoutBox = await ImportAsync(exercise, "61cm", null, [boxStep with { EquipmentNames = [] }]);
        Assert.Contains(Assert.Single(withoutBox.Rows).UnconsumedText, fragment => fragment.Text == "61cm");
    }

    [Fact]
    public async Task ComposesMetconHeadingAndKeepsAmrapWorkAndPace()
    {
        const string details = "amrap40' 30*calc2bike (650 cal/h) + 20*calskierg (600 cal/h)";
        var draft = await ImportAsync("low intensity metcon", details, null,
            [Reference("C2 Bike", WorkTargetType.Calories), Reference("SkiErg", WorkTargetType.Calories)]);
        var block = Assert.Single(draft.Blocks);
        var segment = Assert.Single(block.Segments);

        Assert.Equal(WorkoutBlockType.Metcon, block.BlockType);
        Assert.Equal(WorkoutTrainingIntensity.Low, block.Intensity);
        Assert.Equal(2400, segment.WorkWindowSeconds);
        Assert.Equal(new decimal?[] { 30, 20 }, segment.Steps.Select(step => step.WorkValue));
        Assert.Equal(new decimal?[] { 650, 600 }, segment.Steps.Select(step => step.CaloriesPerHour));
        Assert.Empty(draft.Warnings);
    }

    private static WorkoutImportExerciseReference Reference(string name, WorkTargetType workType) =>
        new()
        {
            Id = new ExerciseId(Guid.NewGuid()),
            Name = name,
            NameKey = FormatingUtilities.NormalizeNameKey(name),
            DefaultWorkTargetType = workType,
            AllowedWorkTargetTypes = [workType]
        };

    private static async Task<WorkoutImportDraft> ImportAsync(
        string exercise,
        string? details,
        string? notes,
        IReadOnlyList<WorkoutImportExerciseReference> exercises)
    {
        var logger = new PRLabLogger();
        logger.ToggleLogging(false);
        var profiles = new JsonWorkoutImportProfileProvider(Options.Create(new WorkoutImportOptions()), logger);
        var profile = await profiles.GetAsync("crossfit-coach");
        var context = new WorkoutImportContext
        {
            UserId = new UserId(Guid.NewGuid()),
            Profile = profile,
            Exercises = exercises
        };

        var row = new ExtractedWorkoutRow
        {
            Cells = [Cell(0, "2026-11-28"), Cell(2, exercise), Cell(3, details), Cell(4, notes)]
        };
        var source = new ExtractedExcelWorkoutSource
        {
            SourceType = WorkoutImportSourceType.File,
            NormalizedText = string.Empty,
            Sheets = [new ExtractedWorkoutSheet { Name = "Coach", RawText = string.Empty, Rows = [row] }]
        };

        var drafts = await new CoachExcelWorkoutImportInterpreter(logger).InterpretAsync(source, context);
        return Assert.Single(drafts);
    }

    private static ExtractedWorkoutCell Cell(int column, string? text) =>
        new() { ColumnIndex = column, Value = text ?? string.Empty, SourceReference = $"column-{column}" };
}
