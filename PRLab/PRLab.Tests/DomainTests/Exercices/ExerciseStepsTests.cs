using FluentAssertions;
using PRLab.Domain.Model.Value;
using PRLab.Domain.Model.Value.Enum.Prescription;
using PRLab.Domain.Model.Value.Enum.Prescription.Load;
using PRLab.Domain.Model.Value.Enum.Prescription.Repetition;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Prescription;
using PRLab.Domain.Model.Value.Prescription.Load;
using PRLab.Domain.Model.Value.Prescription.Rest;
using PRLab.Domain.Model.Value.Prescription.Work;

namespace PRLab.Tests.DomainTests.Exercices;

public sealed class ExerciseStepsTests
{
    [Fact]
    public void New_ShouldCreateExerciseStep_WithProvidedValues()
    {
        var exerciseId = ExerciseId.New();
        var movementId = MovementId.New();
        var sequence = 1;

        var target = WorkTarget.New(
            10m,
            WorkTargetType.Repetitions
        );

        var loadTarget = LoadTarget.ExternalLoad(
            100m,
            LoadUnit.Kilogram
        );

        var restBetweenReps = RestTarget.SecondsDuration(5);
        var transitionAfterStep = RestTarget.SecondsDuration(90);

        var executionDetails = RepExecutionDetails.New(
            eccentricSeconds: 3,
            bottomPauseSeconds: 1,
            concentricSeconds: 2,
            topPauseSeconds: 1,
            eccentricIntent: RepPhaseExecutionIntent.Controlled,
            bottomIntent: RepPhaseExecutionIntent.Paused,
            concentricIntent: RepPhaseExecutionIntent.Explosive,
            topIntent: RepPhaseExecutionIntent.Strict,
            intent: "Keep form strict."
        );

        var Step = ExerciseSteps.New(
            exerciseId,
            movementId,
            sequence,
            target,
            loadTarget,
            restBetweenReps,
            transitionAfterStep,
            executionDetails
        );

        Step.ExerciseId.Should().Be(exerciseId);
        Step.MovementId.Should().Be(movementId);
        Step.Sequence.Should().Be(sequence);
        Step.Target.Should().Be(target);
        Step.LoadTarget.Should().Be(loadTarget);
        Step.RestBetweenReps.Should().Be(restBetweenReps);
        Step.TransitionAfterStep.Should().Be(transitionAfterStep);
        Step.ExecutionDetails.Should().Be(executionDetails);
    }

    [Fact]
    public void New_ShouldCreateExerciseStep_WithDefaultOptionalValues()
    {
        var exerciseId = ExerciseId.New();
        var movementId = MovementId.New();
        var sequence = 1;

        var target = WorkTarget.New(
            10m,
            WorkTargetType.Repetitions
        );

        var Step = ExerciseSteps.New(
            exerciseId,
            movementId,
            sequence,
            target
        );

        Step.ExerciseId.Should().Be(exerciseId);
        Step.MovementId.Should().Be(movementId);
        Step.Sequence.Should().Be(sequence);
        Step.Target.Should().Be(target);

        Step.LoadTarget.Type.Should().Be(LoadTargetType.None);
        Step.LoadTarget.Value.Should().BeNull();
        Step.LoadTarget.Unit.Should().BeNull();

        Step.RestBetweenReps.IsEmpty().Should().BeTrue();
        Step.TransitionAfterStep.IsEmpty().Should().BeTrue();
        Step.ExecutionDetails.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void New_ShouldThrow_WhenTargetIsNull()
    {
        var exerciseId = ExerciseId.New();
        var movementId = MovementId.New();
        var sequence = 1;
        WorkTarget target = null!;

        var act = () => ExerciseSteps.New(
            exerciseId,
            movementId,
            sequence,
            target
        );

        act.Should().Throw<ArgumentNullException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void New_ShouldThrow_WhenSequenceIsLessThanOne(int sequence)
    {
        var exerciseId = ExerciseId.New();
        var movementId = MovementId.New();

        var target = WorkTarget.New(
            10m,
            WorkTargetType.Repetitions
        );

        var act = () => ExerciseSteps.New(
            exerciseId,
            movementId,
            sequence,
            target
        );

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void ChangeTarget_ShouldUpdateTarget()
    {
        var Step = CreateDefaultStep();

        var target = WorkTarget.New(
            30m,
            WorkTargetType.DurationSeconds
        );

        Step.ChangeTarget(target);

        Step.Target.Should().Be(target);
    }

    [Fact]
    public void ChangeTarget_ShouldThrow_WhenTargetIsNull()
    {
        var Step = CreateDefaultStep();
        WorkTarget target = null!;

        var act = () => Step.ChangeTarget(target);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ChangeLoadTarget_ShouldUpdateLoadTarget()
    {
        var Step = CreateDefaultStep();

        var loadTarget = LoadTarget.ExternalLoad(
            80m,
            LoadUnit.Kilogram
        );

        Step.ChangeLoadTarget(loadTarget);

        Step.LoadTarget.Should().Be(loadTarget);
    }

    [Fact]
    public void ChangeLoadTarget_ShouldThrow_WhenLoadTargetIsNull()
    {
        var Step = CreateDefaultStep();
        LoadTarget loadTarget = null!;

        var act = () => Step.ChangeLoadTarget(loadTarget);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RemoveLoadTarget_ShouldSetLoadTargetToNone()
    {
        var Step = CreateDefaultStep();

        var loadTarget = LoadTarget.ExternalLoad(
            80m,
            LoadUnit.Kilogram
        );

        Step.ChangeLoadTarget(loadTarget);

        Step.RemoveLoadTarget();

        Step.LoadTarget.Type.Should().Be(LoadTargetType.None);
        Step.LoadTarget.Value.Should().BeNull();
        Step.LoadTarget.Unit.Should().BeNull();
    }

    [Fact]
    public void ChangeRestBetweenReps_ShouldUpdateRestBetweenReps()
    {
        var Step = CreateDefaultStep();

        var restBetweenReps = RestTarget.SecondsDuration(10);

        Step.ChangeRestBetweenReps(restBetweenReps);

        Step.RestBetweenReps.Should().Be(restBetweenReps);
    }

    [Fact]
    public void ChangeRestBetweenReps_ShouldThrow_WhenRestBetweenRepsIsNull()
    {
        var Step = CreateDefaultStep();
        RestTarget restBetweenReps = null!;

        var act = () => Step.ChangeRestBetweenReps(restBetweenReps);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RemoveRestBetweenReps_ShouldSetRestBetweenRepsToNone()
    {
        var Step = CreateDefaultStep();

        var restBetweenReps = RestTarget.SecondsDuration(10);

        Step.ChangeRestBetweenReps(restBetweenReps);

        Step.RemoveRestBetweenReps();

        Step.RestBetweenReps.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void ChangeTransitionAfterStep_ShouldUpdateTransitionAfterStep()
    {
        var Step = CreateDefaultStep();

        var transitionAfterStep = RestTarget.SecondsDuration(90);

        Step.ChangeTransitionAfterStep(transitionAfterStep);

        Step.TransitionAfterStep.Should().Be(transitionAfterStep);
    }

    [Fact]
    public void ChangeTransitionAfterStep_ShouldThrow_WhenTransitionAfterStepIsNull()
    {
        var Step = CreateDefaultStep();
        RestTarget transitionAfterStep = null!;

        var act = () => Step.ChangeTransitionAfterStep(transitionAfterStep);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RemoveTransitionAfterStep_ShouldSetTransitionAfterStepToNone()
    {
        var Step = CreateDefaultStep();

        var transitionAfterStep = RestTarget.SecondsDuration(90);

        Step.ChangeTransitionAfterStep(transitionAfterStep);

        Step.RemoveTransitionAfterStep();

        Step.TransitionAfterStep.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void ChangeExecutionDetails_ShouldUpdateExecutionDetails()
    {
        var Step = CreateDefaultStep();

        var executionDetails = RepExecutionDetails.New(
            eccentricSeconds: 3,
            concentricSeconds: 1,
            intent: "Explosive up."
        );

        Step.ChangeExecutionDetails(executionDetails);

        Step.ExecutionDetails.Should().Be(executionDetails);
    }

    [Fact]
    public void ChangeExecutionDetails_ShouldThrow_WhenExecutionDetailsIsNull()
    {
        var Step = CreateDefaultStep();
        RepExecutionDetails executionDetails = null!;

        var act = () => Step.ChangeExecutionDetails(executionDetails);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RemoveExecutionDetails_ShouldSetExecutionDetailsToEmpty()
    {
        var Step = CreateDefaultStep();

        var executionDetails = RepExecutionDetails.New(
            eccentricSeconds: 3,
            concentricSeconds: 1,
            intent: "Explosive up."
        );

        Step.ChangeExecutionDetails(executionDetails);

        Step.RemoveExecutionDetails();

        Step.ExecutionDetails.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void ChangeSequence_ShouldUpdateSequence()
    {
        var Step = CreateDefaultStep();
        var sequence = 2;

        Step.ChangeSequence(sequence);

        Step.Sequence.Should().Be(sequence);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ChangeSequence_ShouldThrow_WhenSequenceIsLessThanOne(int sequence)
    {
        var Step = CreateDefaultStep();

        var act = () => Step.ChangeSequence(sequence);

        act.Should().Throw<ArgumentException>();
    }

    private static ExerciseSteps CreateDefaultStep()
    {
        var exerciseId = ExerciseId.New();
        var movementId = MovementId.New();
        var sequence = 1;

        var target = WorkTarget.New(
            10m,
            WorkTargetType.Repetitions
        );

        return ExerciseSteps.New(
            exerciseId,
            movementId,
            sequence,
            target
        );
    }
}