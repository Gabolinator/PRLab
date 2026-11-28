using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using PRLab.Application.Models.ImportDraft;
using PRLab.Application.WorkoutImport.Config;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Model.Value.Enum.Equipment;
using PRLab.Domain.Model.Value.Enum.Prescription.Load;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Enum.Workout;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities;
using PRLab.Infrastructure.DB.Seeding.FromJson.Dtos;
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
        Assert.Equal(new WorkoutWorkImportDraft { Value = 1, SetCount = 16, TargetType = WorkTargetType.Repetitions }, step.Work);
        Assert.Equal(new WorkoutLoadImportDraft { Value = 90, Unit = LoadUnit.Kilogram }, step.Load);
        Assert.Equal(new WorkoutCadenceImportDraft { Type = WorkoutCadenceType.FixedInterval, IntervalSeconds = 50 }, step.Cadence);
        Assert.Null(step.Execution);
        Assert.Null(step.Rate);
        Assert.Null(step.Rest);
    }

    [Fact]
    public async Task ImportsSpeedSquatCuesAndPreservesTheOrderedLoadSeries()
    {
        const string details = "amrap40' | max speed up | paused | oly shoes | on med ball | 1x20-40-60-70-80 eigene zeit";
        var draft = await ImportAsync("speed squat", details, null, [Reference("Squat", WorkTargetType.Repetitions)]);
        var row = Assert.Single(draft.Rows);
        var items = Assert.Single(row.ExerciseInterpretations).SemanticItems;

        Assert.Equal("Speed", Assert.Single(items.OfType<WorkoutMovementQualitySemantic>()).Quality);
        Assert.Null(Assert.Single(items.OfType<WorkoutPauseSemantic>()).Position);
        Assert.Single(items.OfType<WorkoutSelfPacedSemantic>());
        Assert.Contains(items.OfType<WorkoutEquipmentSemantic>(), item => item.Name == "Weightlifting Shoes");
        Assert.Contains(items.OfType<WorkoutEquipmentSemantic>(), item =>
            item is { Name: "Medicine Ball", Relation: WorkoutEquipmentRelation.On });
        var prescriptions = row.Prescriptions.Select(prescription => prescription.Step).ToList();
        var series = prescriptions.Where(step => step.Load is not null).ToList();
        var blockSteps = Assert.Single(Assert.Single(draft.Blocks).Segments).Steps;
        decimal?[] expectedLoads = [20, 40, 60, 70, 80];
        Assert.Equal(expectedLoads, series.Select(step => step.Load?.Value));
        Assert.Equal(expectedLoads, blockSteps.Select(step => step.Load?.Value));
        Assert.All(series, step => Assert.True(step.Work is { SetCount: 1, Value: 1 }));
        var pause = Assert.Single(prescriptions, step => step.Execution?.Pause is not null).Execution!.Pause;
        Assert.Equal(new WorkoutPauseImportDraft(), pause);
        Assert.All(blockSteps, step =>
        {
            Assert.Equal(pause, step.Execution?.Pause);
            Assert.Equal(WorkoutCadenceType.SelfPaced, step.Cadence?.Type);
            Assert.Null(step.Cadence?.IntervalSeconds);
        });
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
        Assert.Equal(new WorkoutWorkImportDraft
        {
            Value = 5,
            TargetType = WorkTargetType.Repetitions,
            Scope = WorkTargetScope.PerSide
        }, step.Work);
        Assert.Equal(new WorkoutHoldImportDraft { DurationSeconds = 5, Scope = WorkoutExecutionScope.Rep }, step.Execution?.Hold);
        var equipment = Assert.Single(items.OfType<WorkoutEquipmentSemantic>());
        Assert.Equal("Plyometric Box", equipment.Name);
        Assert.Equal("on box", equipment.Origin.Source.Text);
        Assert.Contains(row.UnconsumedText, fragment => fragment.Text.Trim().EndsWith("elbow", StringComparison.Ordinal));
    }

    [Fact]
    public async Task UsesRequiredEquipmentCapabilityForHeightAndPreservesZeroLoad()
    {
        const string exercise = "Deep Lateral Box Step Up";
        const string details = "61cm | 1db | 16x0 each side, own time";
        var catalog = ReadEquipmentCatalog();
        var box = catalog.Single(equipment => equipment.Name == "Plyometric Box");
        var boxStep = Reference(exercise, WorkTargetType.Repetitions) with { EquipmentIds = [box.Id] };
        var draft = await ImportAsync(exercise, details, null, [boxStep], catalog);
        var row = Assert.Single(draft.Rows);
        var equipment = Assert.Single(row.ExerciseInterpretations).SemanticItems.OfType<WorkoutEquipmentSemantic>();

        var height = Assert.Single(equipment, item => item.HeightCentimeters.HasValue);
        Assert.Equal(box.Id, height.EquipmentId);
        Assert.Equal(61m, height.HeightCentimeters);
        Assert.Equal("61cm", height.Origin.Source.Text);
        Assert.Contains(equipment, item => item is { Name: "Dumbbell", Quantity: 1 });
        Assert.Contains(row.Prescriptions, entry => entry.Step is
        {
            Work: { Value: 16, Scope: WorkTargetScope.PerSide },
            Load: { Value: 0, Unit: LoadUnit.Kilogram }
        });
        Assert.Contains(row.Prescriptions, entry => entry.Step.Cadence?.Type == WorkoutCadenceType.SelfPaced);

        var withoutBox = await ImportAsync(exercise, "61cm", null, [boxStep with { EquipmentIds = [] }]);
        Assert.Contains(Assert.Single(withoutBox.Rows).UnconsumedText, fragment => fragment.Text == "61cm");
    }

    [Fact]
    public async Task AssociatesHeightWithAnAdjacentEquipmentAliasInEitherOrder()
    {
        var catalog = ReadEquipmentCatalog();
        var box = catalog.Single(equipment => equipment.Name == "Plyometric Box");
        (string Text, int HeightOffset)[] examples = [("box 61cm", 4), ("61cm box", 0)];

        foreach (var example in examples)
        {
            var draft = await ImportAsync(string.Empty, example.Text, null, [], catalog);
            var row = Assert.Single(draft.Rows);
            var height = Assert.Single(row.RowSemanticItems.OfType<WorkoutEquipmentSemantic>(), item => item.HeightCentimeters.HasValue);
            Assert.Equal(box.Id, height.EquipmentId);
            Assert.Equal(61m, height.HeightCentimeters);
            Assert.Equal("61cm", height.Origin.Source.Text);
            Assert.Equal(example.HeightOffset, height.Origin.Source.Offset);
            Assert.Empty(row.UnconsumedText);
        }
    }

    [Fact]
    public async Task LeavesHeightUnconsumedWhenEquipmentDoesNotSupportIt()
    {
        const string name = "Rope Jumps";
        var catalog = ReadEquipmentCatalog();
        var rope = catalog.Single(equipment => equipment.Name == "Jump Rope");
        var exercise = Reference(name, WorkTargetType.Repetitions) with { EquipmentIds = [rope.Id] };
        var draft = await ImportAsync(name, "20cm", null, [exercise], catalog);
        var row = Assert.Single(draft.Rows);

        Assert.Empty(Assert.Single(row.ExerciseInterpretations).SemanticItems.OfType<WorkoutEquipmentSemantic>());
        Assert.Contains(row.UnconsumedText, fragment => fragment.Text == "20cm");
        Assert.Empty(draft.Warnings);
    }

    [Fact]
    public async Task WarnsOnAmbiguousHeightAndUsesAnExplicitNameToDisambiguate()
    {
        const string name = "Platform Step";
        var platform = EquipmentReference("Adjustable Platform") with
        {
            SupportedConfigurationTypes = [EquipmentConfigurationType.Height]
        };
        var bench = EquipmentReference("Adjustable Bench", "bench") with
        {
            SupportedConfigurationTypes = [EquipmentConfigurationType.Height, EquipmentConfigurationType.Angle]
        };
        var exercise = Reference(name, WorkTargetType.Repetitions) with { EquipmentIds = [platform.Id, bench.Id] };
        var ambiguous = await ImportAsync(name, "61cm", null, [exercise], [platform, bench]);
        var ambiguousRow = Assert.Single(ambiguous.Rows);

        Assert.Contains(ambiguousRow.UnconsumedText, fragment => fragment.Text == "61cm");
        Assert.Empty(Assert.Single(ambiguousRow.ExerciseInterpretations).SemanticItems.OfType<WorkoutEquipmentSemantic>());
        var warning = Assert.Single(ambiguous.Warnings);
        Assert.Contains("Ambiguous equipment height '61cm'", warning.Message);
        Assert.Contains(platform.Name, warning.Message);
        Assert.Contains(bench.Name, warning.Message);

        var explicitDraft = await ImportAsync($"{name} bench 61cm", null, null, [exercise], [platform, bench]);
        var explicitExercise = Assert.Single(Assert.Single(explicitDraft.Rows).ExerciseInterpretations);
        var height = Assert.Single(explicitExercise.SemanticItems.OfType<WorkoutEquipmentSemantic>(), item => item.HeightCentimeters.HasValue);
        Assert.Equal(bench.Id, height.EquipmentId);
        Assert.Equal(61m, height.HeightCentimeters);
        Assert.Empty(explicitDraft.Warnings);
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
        Assert.Equal(new decimal?[] { 30, 20 }, segment.Steps.Select(step => step.Work?.Value));
        Assert.Equal(new decimal?[] { 650, 600 }, segment.Steps.Select(step => step.Rate?.Value));
        Assert.All(segment.Steps, step =>
        {
            Assert.Equal(WorkTargetType.Calories, step.Work?.TargetType);
            Assert.True(step.Rate is { Metric: WorkoutRateMetric.Calories, Period: WorkoutRatePeriod.Hour });
        });
        var bike = draft.Rows.Single().ExerciseInterpretations.First();
        var bikeEquipment = Assert.Single(bike.SemanticItems.OfType<WorkoutEquipmentSemantic>());
        Assert.Equal("C2 Bike", bikeEquipment.Name);
        Assert.NotNull(bikeEquipment.EquipmentId);
        Assert.Equal(WorkTargetType.Calories, bikeEquipment.WorkTargetHint);
        Assert.Equal("calc2bike", bikeEquipment.Origin.Source.Text);
        Assert.Empty(draft.Warnings);
    }

    [Fact]
    public async Task ResolvesDomainEquipmentNamesAndQuantitiesWithoutAnExercise()
    {
        var equipment = ReadEquipmentCatalog();
        (string Text, string Name, int? Quantity)[] cases =
        [
            ("DB", "Dumbbell", null),
            ("db", "Dumbbell", null),
            ("Dumb Bell", "Dumbbell", null),
            ("Dumbbell", "Dumbbell", null),
            ("KB", "Kettlebell", null),
            ("MED BALL", "Medicine Ball", null),
            ("Medball", "Medicine Ball", null),
            ("BikeErg", "C2 Bike", null),
            ("Bike Erg", "C2 Bike", null),
            ("SkiErg", "Ski Erg", null),
            ("1db", "Dumbbell", 1),
            ("2 db", "Dumbbell", 2),
            ("1 kb", "Kettlebell", 1),
            ("on med ball", "Medicine Ball", null)
        ];

        foreach (var expected in cases)
        {
            var draft = await ImportAsync(string.Empty, $"setup: {expected.Text}", null, [], equipment);
            var row = Assert.Single(draft.Rows);
            var match = Assert.Single(row.RowSemanticItems.OfType<WorkoutEquipmentSemantic>());
            Assert.Empty(row.ExerciseInterpretations);
            Assert.Equal(expected.Name, match.Name);
            Assert.Equal(equipment.Single(entry => entry.Name == expected.Name).Id, match.EquipmentId);
            Assert.Equal(expected.Quantity, match.Quantity);
            Assert.Equal(expected.Text, match.Origin.Source.Text);
            Assert.Equal(7, match.Origin.Source.Offset);
            Assert.Equal("column-3", match.Origin.Source.Reference);
            Assert.Equal(expected.Text.StartsWith("on ", StringComparison.Ordinal)
                ? WorkoutEquipmentRelation.On
                : (WorkoutEquipmentRelation?)null, match.Relation);
            Assert.DoesNotContain(row.UnconsumedText, fragment => fragment.Text.Contains(expected.Text, StringComparison.Ordinal));
        }
    }

    [Fact]
    public async Task RejectsShortAliasesInsideWordsAndPrefersTheLongestMatch()
    {
        var equipment = ReadEquipmentCatalog()
            .Append(EquipmentReference("Other Ball", "Ball"))
            .ToList();
        var draft = await ImportAsync(string.Empty, "webb addb DBZ pre_db 1dbx | medicine ball", null, [], equipment);
        var row = Assert.Single(draft.Rows);
        var match = Assert.Single(row.RowSemanticItems.OfType<WorkoutEquipmentSemantic>());

        Assert.Equal("Medicine Ball", match.Name);
        Assert.Equal("medicine ball", match.Origin.Source.Text);
        Assert.Contains(row.UnconsumedText, fragment => fragment.Text.Contains("webb addb DBZ pre_db 1dbx", StringComparison.Ordinal));
    }

    [Fact]
    public async Task WarnsOnAmbiguousDomainAliasesAndGivesCanonicalNamesPriority()
    {
        var dumbbell = EquipmentReference("Dumbbell", "DB", "Medicine Ball");
        var other = EquipmentReference("Training Weight", "db");
        var medicineBall = EquipmentReference("Medicine Ball");
        var draft = await ImportAsync(string.Empty, "DB | Medicine Ball", null, [], [dumbbell, other, medicineBall]);
        var row = Assert.Single(draft.Rows);
        var match = Assert.Single(row.RowSemanticItems.OfType<WorkoutEquipmentSemantic>());

        Assert.Equal(medicineBall.Id, match.EquipmentId);
        Assert.Contains(row.UnconsumedText, fragment => fragment.SourceOffset == 0 &&
            fragment.Text.StartsWith("DB |", StringComparison.Ordinal));
        var warning = Assert.Single(draft.Warnings);
        Assert.Contains("Ambiguous equipment 'DB'", warning.Message);
        Assert.Contains("Dumbbell", warning.Message);
        Assert.Contains("Training Weight", warning.Message);
    }

    [Fact]
    public async Task PreservesTopPauseAndBothCadenceCuesInComposedSteps()
    {
        var squat = Reference("Back Squat", WorkTargetType.Repetitions) with { SupportsExternalLoad = true };
        var draft = await ImportAsync("back squat", "amrap40' | paused top | 16x1x90 0:50 | own time", null, [squat]);
        var row = Assert.Single(draft.Rows);
        var pause = Assert.Single(row.Prescriptions, prescription => prescription.Step.Execution?.Pause is not null);
        var step = Assert.Single(Assert.Single(Assert.Single(draft.Blocks).Segments).Steps);

        Assert.Equal("paused top", pause.Step.SourceText);
        Assert.Equal(WorkoutPausePosition.Top, pause.Step.Execution?.Pause?.Position);
        Assert.Equal(pause.Step.Execution?.Pause, step.Execution?.Pause);
        Assert.Equal(WorkoutCadenceType.FixedInterval | WorkoutCadenceType.SelfPaced, step.Cadence?.Type);
        Assert.Equal(50, step.Cadence?.IntervalSeconds);
    }

    [Fact]
    public async Task PreservesExistingWorkLoadAndRestInterpretations()
    {
        (string Text, WorkTargetType Type, decimal Value, int? Sets, decimal? Load)[] examples =
        [
            ("25", WorkTargetType.Repetitions, 25, null, null),
            ("4x8", WorkTargetType.Repetitions, 8, 4, null),
            ("1x90s", WorkTargetType.DurationSeconds, 90, 1, null),
            ("45x15 split anyhow", WorkTargetType.Repetitions, 45, null, 15),
            ("50x17.5 split anyhow", WorkTargetType.Repetitions, 50, null, 17.5m)
        ];

        foreach (var example in examples)
        {
            var exercise = Reference("Squat", example.Type) with { SupportsExternalLoad = true };
            var draft = await ImportAsync("squat", example.Text, null, [exercise]);
            var row = Assert.Single(draft.Rows);
            var step = Assert.Single(row.Prescriptions, prescription => prescription.Step.Work is not null).Step;
            Assert.Equal(example.Value, step.Work!.Value);
            Assert.Equal(example.Type, step.Work.TargetType);
            Assert.Equal(example.Sets, step.Work.SetCount);
            Assert.Equal(example.Load, step.Load?.Value);
            Assert.Equal(example.Load.HasValue ? LoadUnit.Kilogram : (LoadUnit?)null, step.Load?.Unit);
        }

        var loadAndRest = await ImportAsync(
            "squat",
            "20kg | rest 120s | 120s+ rest",
            null,
            [Reference("Squat", WorkTargetType.Repetitions)]);
        var steps =
            Assert.Single(loadAndRest.Rows).Prescriptions
                .Select(prescription => prescription.Step)
                .ToList();

        var load = Assert.Single(steps, step => step.Load is not null);
        Assert.Equal(new WorkoutLoadImportDraft { Value = 20, Unit = LoadUnit.Kilogram }, load.Load);
        Assert.Null(load.Work);
        Assert.Collection(
            steps.Where(step => step.StepKind == WorkoutStepKind.Rest),
            exact => Assert.Equal(new WorkoutRestImportDraft { Seconds = 120 }, exact.Rest),
            minimum => Assert.Equal(new WorkoutRestImportDraft { MinimumSeconds = 120 }, minimum.Rest));
    }

    private static WorkoutImportEquipmentReference EquipmentReference(string name, params string[] aliases) =>
        new()
        {
            Id = EquipmentId.New(),
            Name = name,
            NameKey = FormatingUtilities.NormalizeNameKey(name),
            Aliases = aliases
        };

    private static IReadOnlyList<WorkoutImportEquipmentReference> ReadEquipmentCatalog()
    {
        var json = File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "SeedData", "equipment.seed.json"));
        var options = new JsonSerializerOptions { Converters = { new JsonStringEnumConverter() } };
        var entries = JsonSerializer.Deserialize<EquipmentSeedJsonDto[]>(json, options)!;

        return entries
            .Select(entry => new WorkoutImportEquipmentReference
            {
                Id = EquipmentId.FromGuid(entry.Id!.Value),
                Name = entry.Name,
                NameKey = entry.NameKey,
                Aliases = entry.Aliases,
                SupportedConfigurationTypes = entry.SupportedConfigurationTypes
            })
            .ToList();
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
        IReadOnlyList<WorkoutImportExerciseReference> exercises,
        IReadOnlyList<WorkoutImportEquipmentReference>? equipment = null)
    {
        var logger = new PRLabLogger();
        logger.ToggleLogging(false);
        var profiles = new JsonWorkoutImportProfileProvider(Options.Create(new WorkoutImportOptions()), logger);
        var profile = await profiles.GetAsync("crossfit-coach");
        var context = new WorkoutImportContext
        {
            UserId = new UserId(Guid.NewGuid()),
            Profile = profile,
            Exercises = exercises,
            Equipment = equipment ?? ReadEquipmentCatalog()
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
