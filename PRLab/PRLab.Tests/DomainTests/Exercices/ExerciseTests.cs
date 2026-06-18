using FluentAssertions;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Interface;
using PRLab.Domain.Model.Value;
using PRLab.Domain.Model.Value.Enum.Movement;
using PRLab.Domain.Model.Value.Enum.Prescription;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Prescription;
using PRLab.Domain.Utilities;

namespace PRLab.Tests.DomainTests.Exercices;

public sealed class ExerciseTests
{
    [Fact]
    public void NewBuiltIn_ShouldCreateExercise_WithNormalizedNameAndNameKey()
    {
        var name = "  Pull-up Exercise  ";
        var descriptionText = "Strict pull-up exercise.";

        var exercise = Exercise.NewBuiltIn(
            name,
            descriptionText
        );

        exercise.Name.Should().Be(FormatingUtilities.NormalizeName(name));
        exercise.NameKey.Should().Be(FormatingUtilities.NormalizeNameKey(name));
        exercise.Description.Should().NotBeNull();
        exercise.Description.GetContent().Should().Be(descriptionText);
        exercise.Audit.Should().NotBeNull();
        exercise.Audit.IsDeleted.Should().BeFalse();
        exercise.Ownership.Should().NotBeNull();
        exercise.Ownership.Origin.Should().Be(DataOrigin.BuiltIn);
        exercise.Ownership.OwnerUserId.Should().BeNull();
        exercise.Steps.Should().BeEmpty();
    }

    [Fact]
    public void NewBuiltIn_ShouldCreateExercise_WithEmptyDescription_WhenDescriptionIsNull()
    {
        var name = "Pull-up Exercise";
        string? descriptionText = null;

        var exercise = Exercise.NewBuiltIn(
            name,
            descriptionText
        );

        exercise.Name.Should().Be(FormatingUtilities.NormalizeName(name));
        exercise.NameKey.Should().Be(FormatingUtilities.NormalizeNameKey(name));
        exercise.Description.Should().NotBeNull();
        exercise.Description.GetContent().Should().BeNull();
        exercise.Ownership.Should().NotBeNull();
        exercise.Ownership.Origin.Should().Be(DataOrigin.BuiltIn);
        exercise.Ownership.OwnerUserId.Should().BeNull();
        exercise.Steps.Should().BeEmpty();
    }

    [Fact]
    public void NewBuiltIn_ShouldThrow_WhenNameIsEmpty()
    {
        var name = "   ";
        var descriptionText = "Invalid exercise.";

        var act = () => Exercise.NewBuiltIn(
            name,
            descriptionText
        );

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NewUserCreated_ShouldCreateExercise_WithOwner()
    {
        var owner = User.New("Test User");
        var name = "  Custom Pull-up Exercise  ";
        var descriptionText = "User-created pull-up exercise.";

        var exercise = Exercise.NewUserCreated(
            name,
            descriptionText,
            owner
        );

        exercise.Name.Should().Be(FormatingUtilities.NormalizeName(name));
        exercise.NameKey.Should().Be(FormatingUtilities.NormalizeNameKey(name));
        exercise.Description.GetContent().Should().Be(descriptionText);
        exercise.Audit.Should().NotBeNull();
        exercise.Audit.CreatedBy.Should().Be(owner.Id);
        exercise.Ownership.Should().NotBeNull();
        exercise.Ownership.Origin.Should().Be(DataOrigin.UserCreated);
        exercise.Ownership.OwnerUserId.Should().Be(owner.Id);
        exercise.Steps.Should().BeEmpty();
    }

    [Fact]
    public void Rename_ShouldUpdateNameAndNameKey()
    {
        var initialName = "Pull-up";
        var newName = "  Strict Pull-up  ";

        var exercise = Exercise.NewBuiltIn(
            initialName,
            Description.None(),
            null
        );

        exercise.Rename(newName);

        exercise.Name.Should().Be(FormatingUtilities.NormalizeName(newName));
        exercise.NameKey.Should().Be(FormatingUtilities.NormalizeNameKey(newName));
    }

    [Fact]
    public void Rename_ShouldMarkExerciseAsUpdated()
    {
        var initialName = "Pull-up";
        var newName = "Strict Pull-up";

        var exercise = Exercise.NewBuiltIn(
            initialName,
            Description.None(),
            null
        );

        var previousUpdatedAt = exercise.Audit.UpdatedAt;

        exercise.Rename(newName);

        exercise.Audit.UpdatedAt.Should().NotBe(previousUpdatedAt);
    }

    [Fact]
    public void FromMovementBuiltIn_ShouldCreateExercise_FromMovement()
    {
        var movementName = "  Pull-up  ";
        var movementDescriptionText = "Vertical pulling movement.";
        var movementCategoryId = MovementCategoryId.New();
        var value = 5m;
        var targetType = WorkTargetType.Repetitions;

        var movement = Movement.NewBuiltIn(
            movementName,
            movementCategoryId,
            movementDescriptionText,
            defaultWorkTargetType: WorkTargetType.Repetitions,
            laterality: MovementLaterality.Bilateral
        );

        var exercise = Exercise.FromMovementBuiltIn(
            movement,
            value,
            targetType
        );

        exercise.Name.Should().Be(movement.Name);
        exercise.NameKey.Should().Be(movement.NameKey);
        exercise.Description.Id.Should().NotBe(movement.Description.Id);
        exercise.Description.GetContent().Should().Be(movementDescriptionText);
        exercise.Ownership.Should().NotBeNull();
        exercise.Ownership.Origin.Should().Be(DataOrigin.BuiltIn);
        exercise.Ownership.OwnerUserId.Should().BeNull();
        exercise.Steps.Should().ContainSingle();

        var step = exercise.Steps.First();

        step.ExerciseId.Should().Be(exercise.Id);
        step.MovementId.Should().Be(movement.Id);
        step.Sequence.Should().Be(1);
        step.Target.Value.Should().Be(value);
        step.Target.TargetType.Should().Be(targetType);
        step.LoadTarget.Type.Should().Be(LoadTargetType.None);
        step.RestBetweenReps.IsEmpty().Should().BeTrue();
        step.TransitionAfterStep.IsEmpty().Should().BeTrue();
        step.ExecutionDetails.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void FromMovementUserCreated_ShouldCreateExercise_WithOwner()
    {
        var owner = User.New("Test User");
        var movementName = "  Pull-up  ";
        var movementDescriptionText = "Vertical pulling movement.";
        var movementCategoryId = MovementCategoryId.New();
        var value = 5m;
        var targetType = WorkTargetType.Repetitions;

        var movement = Movement.NewBuiltIn(
            movementName,
            movementCategoryId,
            movementDescriptionText,
            defaultWorkTargetType: WorkTargetType.Repetitions,
            laterality: MovementLaterality.Bilateral
        );

        var exercise = Exercise.FromMovementUserCreated(
            movement,
            value,
            targetType,
            owner
        );

        exercise.Name.Should().Be(movement.Name);
        exercise.NameKey.Should().Be(movement.NameKey);
        exercise.Ownership.Should().NotBeNull();
        exercise.Ownership.Origin.Should().Be(DataOrigin.UserCreated);
        exercise.Ownership.OwnerUserId.Should().Be(owner.Id);
        exercise.Audit.CreatedBy.Should().Be(owner.Id);
        exercise.Steps.Should().ContainSingle();
    }

    [Fact]
    public void FromMovementBuiltIn_ShouldCreateExercise_WithProvidedOptionalTargets()
    {
        var owner = User.New("Test User");
        var movementName = "  Weighted Pull-up  ";
        var movementDescriptionText = "Vertical pulling movement with external load.";
        var movementCategoryId = MovementCategoryId.New();
        var value = 5m;
        var targetType = WorkTargetType.Repetitions;

        var loadTarget = LoadTarget.AddedBodyWeightLoad(
            20m,
            LoadUnit.Kilogram
        );

        var restBetweenReps = RestTarget.SecondsDuration(5);
        var transitionAfterStep = RestTarget.SecondsDuration(120);

        var executionDetails = RepExecutionDetails.New(
            eccentricSeconds: 3,
            bottomPauseSeconds: 1,
            concentricSeconds: 1,
            topPauseSeconds: 0,
            eccentricIntent: RepPhaseExecutionIntent.Controlled,
            bottomIntent: RepPhaseExecutionIntent.Paused,
            concentricIntent: RepPhaseExecutionIntent.Explosive,
            topIntent: RepPhaseExecutionIntent.Strict,
            intent: "Keep the reps clean."
        );

        var movement = Movement.NewBuiltIn(
            movementName,
            movementCategoryId,
            movementDescriptionText,
            defaultWorkTargetType: WorkTargetType.Repetitions,
            laterality: MovementLaterality.Bilateral
        );

        var exercise = Exercise.FromMovementBuiltIn(
            movement,
            value,
            targetType,
            owner,
            loadTarget,
            restBetweenReps,
            transitionAfterStep,
            executionDetails
        );

        var step = exercise.Steps.First();

        step.Target.Value.Should().Be(value);
        step.Target.TargetType.Should().Be(targetType);
        step.LoadTarget.Should().Be(loadTarget);
        step.RestBetweenReps.Should().Be(restBetweenReps);
        step.TransitionAfterStep.Should().Be(transitionAfterStep);
        step.ExecutionDetails.Should().Be(executionDetails);
    }

    [Fact]
    public void FromMovementBuiltIn_ShouldThrow_WhenMovementIsNull()
    {
        Movement movement = null!;
        var value = 5m;
        var targetType = WorkTargetType.Repetitions;

        var act = () => Exercise.FromMovementBuiltIn(
            movement,
            value,
            targetType
        );

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddStep_ShouldAddStep_WithNextSequence()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        var firstMovementId = MovementId.New();
        var secondMovementId = MovementId.New();

        exercise.AddStep(
            firstMovementId,
            5m,
            WorkTargetType.Repetitions
        );

        exercise.AddStep(
            secondMovementId,
            10m,
            WorkTargetType.Repetitions
        );

        exercise.Steps.Should().HaveCount(2);

        exercise.Steps.ElementAt(0).MovementId.Should().Be(firstMovementId);
        exercise.Steps.ElementAt(0).Sequence.Should().Be(1);

        exercise.Steps.ElementAt(1).MovementId.Should().Be(secondMovementId);
        exercise.Steps.ElementAt(1).Sequence.Should().Be(2);
    }

    [Fact]
    public void AddStep_ShouldAddStep_WithProvidedOptionalTargets()
    {
        var exercise = Exercise.NewBuiltIn(
            "Weighted Pull-up Exercise",
            Description.None(),
            null
        );

        var movementId = MovementId.New();
        var value = 5m;
        var targetType = WorkTargetType.Repetitions;

        var loadTarget = LoadTarget.AddedBodyWeightLoad(
            20m,
            LoadUnit.Kilogram
        );

        var restBetweenReps = RestTarget.SecondsDuration(5);
        var transitionAfterStep = RestTarget.SecondsDuration(120);

        var executionDetails = RepExecutionDetails.New(
            eccentricSeconds: 3,
            concentricSeconds: 1,
            intent: "Controlled eccentric, explosive concentric."
        );

        exercise.AddStep(
            movementId,
            value,
            targetType,
            loadTarget,
            restBetweenReps,
            transitionAfterStep,
            executionDetails
        );

        var step = exercise.Steps.First();

        step.MovementId.Should().Be(movementId);
        step.Sequence.Should().Be(1);
        step.Target.Value.Should().Be(value);
        step.Target.TargetType.Should().Be(targetType);
        step.LoadTarget.Should().Be(loadTarget);
        step.RestBetweenReps.Should().Be(restBetweenReps);
        step.TransitionAfterStep.Should().Be(transitionAfterStep);
        step.ExecutionDetails.Should().Be(executionDetails);
    }

    [Fact]
    public void AddStep_ShouldMarkExerciseAsUpdated()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        var previousUpdatedAt = exercise.Audit.UpdatedAt;

        exercise.AddStep(
            MovementId.New(),
            5m,
            WorkTargetType.Repetitions
        );

        exercise.Audit.UpdatedAt.Should().NotBe(previousUpdatedAt);
    }

    [Fact]
    public void AddStep_ShouldThrow_WhenWorkTargetValueIsNotGreaterThanZero()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        var movementId = MovementId.New();
        var value = 0m;
        var targetType = WorkTargetType.Repetitions;

        var act = () => exercise.AddStep(
            movementId,
            value,
            targetType
        );

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RemoveStep_ShouldRemoveStep_WhenStepExists()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        exercise.AddStep(
            MovementId.New(),
            5m,
            WorkTargetType.Repetitions
        );

        var stepId = exercise.Steps.First().Id;

        exercise.RemoveStep(stepId);

        exercise.Steps.Should().BeEmpty();
    }

    [Fact]
    public void RemoveStep_ShouldDoNothing_WhenStepDoesNotExist()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        var exerciseStepId = ExerciseStepsId.New();

        exercise.RemoveStep(exerciseStepId);

        exercise.Steps.Should().BeEmpty();
    }

    [Fact]
    public void RemoveStep_ShouldResequenceRemainingSteps()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        exercise.AddStep(
            MovementId.New(),
            5m,
            WorkTargetType.Repetitions
        );

        exercise.AddStep(
            MovementId.New(),
            10m,
            WorkTargetType.Repetitions
        );

        exercise.AddStep(
            MovementId.New(),
            15m,
            WorkTargetType.Repetitions
        );

        var secondStepId = exercise.Steps.ElementAt(1).Id;

        exercise.RemoveStep(secondStepId);

        exercise.Steps.Should().HaveCount(2);
        exercise.Steps.ElementAt(0).Sequence.Should().Be(1);
        exercise.Steps.ElementAt(1).Sequence.Should().Be(2);
    }

    [Fact]
    public void MoveStep_ShouldMoveStepAndResequenceSteps()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        var firstMovementId = MovementId.New();
        var secondMovementId = MovementId.New();
        var thirdMovementId = MovementId.New();

        exercise.AddStep(
            firstMovementId,
            5m,
            WorkTargetType.Repetitions
        );

        exercise.AddStep(
            secondMovementId,
            10m,
            WorkTargetType.Repetitions
        );

        exercise.AddStep(
            thirdMovementId,
            15m,
            WorkTargetType.Repetitions
        );

        var thirdStepId = exercise.Steps.ElementAt(2).Id;

        exercise.MoveStep(
            thirdStepId,
            1
        );

        exercise.Steps.ElementAt(0).MovementId.Should().Be(thirdMovementId);
        exercise.Steps.ElementAt(0).Sequence.Should().Be(1);

        exercise.Steps.ElementAt(1).Sequence.Should().Be(2);
        exercise.Steps.ElementAt(2).Sequence.Should().Be(3);
    }

    [Fact]
    public void MoveStep_ShouldThrow_WhenNewSequenceIsLessThanOne()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        exercise.AddStep(
            MovementId.New(),
            5m,
            WorkTargetType.Repetitions
        );

        var stepId = exercise.Steps.First().Id;

        var act = () => exercise.MoveStep(
            stepId,
            0
        );

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MoveStep_ShouldDoNothing_WhenStepDoesNotExist()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        exercise.AddStep(
            MovementId.New(),
            5m,
            WorkTargetType.Repetitions
        );

        var missingStepId = ExerciseStepsId.New();

        exercise.MoveStep(
            missingStepId,
            1
        );

        exercise.Steps.Should().ContainSingle();
        exercise.Steps.First().Sequence.Should().Be(1);
    }

    [Fact]
    public void ChangeDescription_ShouldChangeDescriptionContent()
    {
        var initialDescriptionText = "Initial description.";
        var newDescriptionText = "Updated description.";
        var language = LocalizationHelper.Language.EN;

        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            initialDescriptionText
        );

        exercise.ChangeDescription(
            newDescriptionText,
            language
        );

        exercise.Description.GetContent(language).Should().Be(newDescriptionText);
    }

    [Fact]
    public void ChangeDescription_ShouldMarkExerciseAsUpdated()
    {
        var initialDescriptionText = "Initial description.";
        var newDescriptionText = "Updated description.";
        var language = LocalizationHelper.Language.EN;

        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            initialDescriptionText
        );

        var previousUpdatedAt = exercise.Audit.UpdatedAt;

        exercise.ChangeDescription(
            newDescriptionText,
            language
        );

        exercise.Audit.UpdatedAt.Should().NotBe(previousUpdatedAt);
    }

    [Fact]
    public void RemoveDescription_ShouldRemoveDescriptionContent()
    {
        var descriptionText = "Initial description.";
        var language = LocalizationHelper.Language.EN;

        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            descriptionText
        );

        exercise.RemoveDescription(language);

        exercise.Description.GetContent(language).Should().BeNull();
    }

    [Fact]
    public void MarkDeleted_ShouldMarkExerciseAsDeleted()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        exercise.MarkDeleted();

        exercise.Audit.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void MarkDeleted_ShouldMarkExerciseAsDeleted_WhenCalledThroughAuditedInterface()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        ((IAudited)exercise).MarkDeleted();

        exercise.Audit.IsDeleted.Should().BeTrue();
    }

    [Fact]
    public void ChangeStepTarget_ShouldChangeTarget_WhenStepExists()
    {
        var exercise = CreateExerciseWithOneStep();

        var stepId = exercise.Steps.First().Id;
        var newValue = 30m;
        var newTargetType = WorkTargetType.DurationSeconds;

        exercise.ChangeStepTarget(
            stepId,
            newValue,
            newTargetType
        );

        exercise.Steps.First().Target.Value.Should().Be(newValue);
        exercise.Steps.First().Target.TargetType.Should().Be(newTargetType);
    }

    [Fact]
    public void ChangeStepTarget_ShouldDoNothing_WhenStepDoesNotExist()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        var missingStepId = ExerciseStepsId.New();

        exercise.ChangeStepTarget(
            missingStepId,
            30m,
            WorkTargetType.DurationSeconds
        );

        exercise.Steps.Should().BeEmpty();
    }

    [Fact]
    public void ChangeStepLoadTarget_ShouldChangeLoadTarget_WhenStepExists()
    {
        var exercise = CreateExerciseWithOneStep();
        var stepId = exercise.Steps.First().Id;

        var loadTarget = LoadTarget.ExternalLoad(
            80m,
            LoadUnit.Kilogram
        );

        exercise.ChangeStepLoadTarget(
            stepId,
            loadTarget
        );

        exercise.Steps.First().LoadTarget.Should().Be(loadTarget);
    }

    [Fact]
    public void ChangeStepLoadTarget_ShouldDoNothing_WhenStepDoesNotExist()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        var missingStepId = ExerciseStepsId.New();

        var loadTarget = LoadTarget.ExternalLoad(
            80m,
            LoadUnit.Kilogram
        );

        exercise.ChangeStepLoadTarget(
            missingStepId,
            loadTarget
        );

        exercise.Steps.Should().BeEmpty();
    }

    [Fact]
    public void RemoveStepLoadTarget_ShouldSetLoadTargetToNone_WhenStepExists()
    {
        var exercise = CreateExerciseWithOneStep();
        var stepId = exercise.Steps.First().Id;

        var loadTarget = LoadTarget.ExternalLoad(
            80m,
            LoadUnit.Kilogram
        );

        exercise.ChangeStepLoadTarget(
            stepId,
            loadTarget
        );

        exercise.RemoveStepLoadTarget(stepId);

        exercise.Steps.First().LoadTarget.Type.Should().Be(LoadTargetType.None);
        exercise.Steps.First().LoadTarget.Value.Should().BeNull();
        exercise.Steps.First().LoadTarget.Unit.Should().BeNull();
    }

    [Fact]
    public void ChangeStepRestBetweenReps_ShouldChangeRestBetweenReps_WhenStepExists()
    {
        var exercise = CreateExerciseWithOneStep();
        var stepId = exercise.Steps.First().Id;
        var restBetweenReps = RestTarget.SecondsDuration(10);

        exercise.ChangeStepRestBetweenReps(
            stepId,
            restBetweenReps
        );

        exercise.Steps.First().RestBetweenReps.Should().Be(restBetweenReps);
    }

    [Fact]
    public void RemoveStepRestBetweenReps_ShouldSetRestBetweenRepsToNone_WhenStepExists()
    {
        var exercise = CreateExerciseWithOneStep();
        var stepId = exercise.Steps.First().Id;
        var restBetweenReps = RestTarget.SecondsDuration(10);

        exercise.ChangeStepRestBetweenReps(
            stepId,
            restBetweenReps
        );

        exercise.RemoveStepRestBetweenReps(stepId);

        exercise.Steps.First().RestBetweenReps.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void ChangeStepTransitionAfterStep_ShouldChangeTransitionAfterStep_WhenStepExists()
    {
        var exercise = CreateExerciseWithOneStep();
        var stepId = exercise.Steps.First().Id;
        var transitionAfterStep = RestTarget.SecondsDuration(120);

        exercise.ChangeStepTransitionAfterStep(
            stepId,
            transitionAfterStep
        );

        exercise.Steps.First().TransitionAfterStep.Should().Be(transitionAfterStep);
    }

    [Fact]
    public void RemoveStepTransitionAfterStep_ShouldSetTransitionAfterStepToNone_WhenStepExists()
    {
        var exercise = CreateExerciseWithOneStep();
        var stepId = exercise.Steps.First().Id;
        var transitionAfterStep = RestTarget.SecondsDuration(120);

        exercise.ChangeStepTransitionAfterStep(
            stepId,
            transitionAfterStep
        );

        exercise.RemoveStepTransitionAfterStep(stepId);

        exercise.Steps.First().TransitionAfterStep.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void ChangeStepExecutionDetails_ShouldChangeExecutionDetails_WhenStepExists()
    {
        var exercise = CreateExerciseWithOneStep();
        var stepId = exercise.Steps.First().Id;

        var executionDetails = RepExecutionDetails.New(
            eccentricSeconds: 3,
            concentricSeconds: 1,
            intent: "Controlled eccentric."
        );

        exercise.ChangeStepExecutionDetails(
            stepId,
            executionDetails
        );

        exercise.Steps.First().ExecutionDetails.Should().Be(executionDetails);
    }

    [Fact]
    public void RemoveStepExecutionDetails_ShouldSetExecutionDetailsToEmpty_WhenStepExists()
    {
        var exercise = CreateExerciseWithOneStep();
        var stepId = exercise.Steps.First().Id;

        var executionDetails = RepExecutionDetails.New(
            eccentricSeconds: 3,
            concentricSeconds: 1,
            intent: "Controlled eccentric."
        );

        exercise.ChangeStepExecutionDetails(
            stepId,
            executionDetails
        );

        exercise.RemoveStepExecutionDetails(stepId);

        exercise.Steps.First().ExecutionDetails.IsEmpty().Should().BeTrue();
    }

    private static Exercise CreateExerciseWithOneStep()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        exercise.AddStep(
            MovementId.New(),
            5m,
            WorkTargetType.Repetitions
        );

        return exercise;
    }
}