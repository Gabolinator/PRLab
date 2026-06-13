using FluentAssertions;
using PRLab.Domain.Value;
using PRLab.Domain.Value.Enum.Prescription;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Tests.DomainTests.Exercices;

public sealed class ExerciseBlockTests
{
    [Fact]
    public void New_ShouldCreateExerciseBlock_WithProvidedValues()
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
        var transitionAfterBlock = RestTarget.SecondsDuration(90);

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

        var block = ExerciseBlock.New(
            exerciseId,
            movementId,
            sequence,
            target,
            loadTarget,
            restBetweenReps,
            transitionAfterBlock,
            executionDetails
        );

        block.ExerciseId.Should().Be(exerciseId);
        block.MovementId.Should().Be(movementId);
        block.Sequence.Should().Be(sequence);
        block.Target.Should().Be(target);
        block.LoadTarget.Should().Be(loadTarget);
        block.RestBetweenReps.Should().Be(restBetweenReps);
        block.TransitionAfterBlock.Should().Be(transitionAfterBlock);
        block.ExecutionDetails.Should().Be(executionDetails);
    }

    [Fact]
    public void New_ShouldCreateExerciseBlock_WithDefaultOptionalValues()
    {
        var exerciseId = ExerciseId.New();
        var movementId = MovementId.New();
        var sequence = 1;

        var target = WorkTarget.New(
            10m,
            WorkTargetType.Repetitions
        );

        var block = ExerciseBlock.New(
            exerciseId,
            movementId,
            sequence,
            target
        );

        block.ExerciseId.Should().Be(exerciseId);
        block.MovementId.Should().Be(movementId);
        block.Sequence.Should().Be(sequence);
        block.Target.Should().Be(target);

        block.LoadTarget.Type.Should().Be(LoadTargetType.None);
        block.LoadTarget.Value.Should().BeNull();
        block.LoadTarget.Unit.Should().BeNull();

        block.RestBetweenReps.IsEmpty().Should().BeTrue();
        block.TransitionAfterBlock.IsEmpty().Should().BeTrue();
        block.ExecutionDetails.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void New_ShouldThrow_WhenTargetIsNull()
    {
        var exerciseId = ExerciseId.New();
        var movementId = MovementId.New();
        var sequence = 1;
        WorkTarget target = null!;

        var act = () => ExerciseBlock.New(
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

        var act = () => ExerciseBlock.New(
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
        var block = CreateDefaultBlock();

        var target = WorkTarget.New(
            30m,
            WorkTargetType.DurationSeconds
        );

        block.ChangeTarget(target);

        block.Target.Should().Be(target);
    }

    [Fact]
    public void ChangeTarget_ShouldThrow_WhenTargetIsNull()
    {
        var block = CreateDefaultBlock();
        WorkTarget target = null!;

        var act = () => block.ChangeTarget(target);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ChangeLoadTarget_ShouldUpdateLoadTarget()
    {
        var block = CreateDefaultBlock();

        var loadTarget = LoadTarget.ExternalLoad(
            80m,
            LoadUnit.Kilogram
        );

        block.ChangeLoadTarget(loadTarget);

        block.LoadTarget.Should().Be(loadTarget);
    }

    [Fact]
    public void ChangeLoadTarget_ShouldThrow_WhenLoadTargetIsNull()
    {
        var block = CreateDefaultBlock();
        LoadTarget loadTarget = null!;

        var act = () => block.ChangeLoadTarget(loadTarget);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RemoveLoadTarget_ShouldSetLoadTargetToNone()
    {
        var block = CreateDefaultBlock();

        var loadTarget = LoadTarget.ExternalLoad(
            80m,
            LoadUnit.Kilogram
        );

        block.ChangeLoadTarget(loadTarget);

        block.RemoveLoadTarget();

        block.LoadTarget.Type.Should().Be(LoadTargetType.None);
        block.LoadTarget.Value.Should().BeNull();
        block.LoadTarget.Unit.Should().BeNull();
    }

    [Fact]
    public void ChangeRestBetweenReps_ShouldUpdateRestBetweenReps()
    {
        var block = CreateDefaultBlock();

        var restBetweenReps = RestTarget.SecondsDuration(10);

        block.ChangeRestBetweenReps(restBetweenReps);

        block.RestBetweenReps.Should().Be(restBetweenReps);
    }

    [Fact]
    public void ChangeRestBetweenReps_ShouldThrow_WhenRestBetweenRepsIsNull()
    {
        var block = CreateDefaultBlock();
        RestTarget restBetweenReps = null!;

        var act = () => block.ChangeRestBetweenReps(restBetweenReps);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RemoveRestBetweenReps_ShouldSetRestBetweenRepsToNone()
    {
        var block = CreateDefaultBlock();

        var restBetweenReps = RestTarget.SecondsDuration(10);

        block.ChangeRestBetweenReps(restBetweenReps);

        block.RemoveRestBetweenReps();

        block.RestBetweenReps.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void ChangeTransitionAfterBlock_ShouldUpdateTransitionAfterBlock()
    {
        var block = CreateDefaultBlock();

        var transitionAfterBlock = RestTarget.SecondsDuration(90);

        block.ChangeTransitionAfterBlock(transitionAfterBlock);

        block.TransitionAfterBlock.Should().Be(transitionAfterBlock);
    }

    [Fact]
    public void ChangeTransitionAfterBlock_ShouldThrow_WhenTransitionAfterBlockIsNull()
    {
        var block = CreateDefaultBlock();
        RestTarget transitionAfterBlock = null!;

        var act = () => block.ChangeTransitionAfterBlock(transitionAfterBlock);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RemoveTransitionAfterBlock_ShouldSetTransitionAfterBlockToNone()
    {
        var block = CreateDefaultBlock();

        var transitionAfterBlock = RestTarget.SecondsDuration(90);

        block.ChangeTransitionAfterBlock(transitionAfterBlock);

        block.RemoveTransitionAfterBlock();

        block.TransitionAfterBlock.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void ChangeExecutionDetails_ShouldUpdateExecutionDetails()
    {
        var block = CreateDefaultBlock();

        var executionDetails = RepExecutionDetails.New(
            eccentricSeconds: 3,
            concentricSeconds: 1,
            intent: "Explosive up."
        );

        block.ChangeExecutionDetails(executionDetails);

        block.ExecutionDetails.Should().Be(executionDetails);
    }

    [Fact]
    public void ChangeExecutionDetails_ShouldThrow_WhenExecutionDetailsIsNull()
    {
        var block = CreateDefaultBlock();
        RepExecutionDetails executionDetails = null!;

        var act = () => block.ChangeExecutionDetails(executionDetails);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void RemoveExecutionDetails_ShouldSetExecutionDetailsToEmpty()
    {
        var block = CreateDefaultBlock();

        var executionDetails = RepExecutionDetails.New(
            eccentricSeconds: 3,
            concentricSeconds: 1,
            intent: "Explosive up."
        );

        block.ChangeExecutionDetails(executionDetails);

        block.RemoveExecutionDetails();

        block.ExecutionDetails.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void ChangeSequence_ShouldUpdateSequence()
    {
        var block = CreateDefaultBlock();
        var sequence = 2;

        block.ChangeSequence(sequence);

        block.Sequence.Should().Be(sequence);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ChangeSequence_ShouldThrow_WhenSequenceIsLessThanOne(int sequence)
    {
        var block = CreateDefaultBlock();

        var act = () => block.ChangeSequence(sequence);

        act.Should().Throw<ArgumentException>();
    }

    private static ExerciseBlock CreateDefaultBlock()
    {
        var exerciseId = ExerciseId.New();
        var movementId = MovementId.New();
        var sequence = 1;

        var target = WorkTarget.New(
            10m,
            WorkTargetType.Repetitions
        );

        return ExerciseBlock.New(
            exerciseId,
            movementId,
            sequence,
            target
        );
    }
}