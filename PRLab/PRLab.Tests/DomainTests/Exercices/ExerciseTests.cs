using FluentAssertions;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Interface;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value;
using PRLab.Domain.Value.Identifier;

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
        exercise.Ownership.Origin.Should().Be(DomainEnum.DataOrigin.BuiltIn);
        exercise.Ownership.OwnerUserId.Should().BeNull();
        exercise.Blocks.Should().BeEmpty();
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
        exercise.Ownership.Origin.Should().Be(DomainEnum.DataOrigin.BuiltIn);
        exercise.Ownership.OwnerUserId.Should().BeNull();
        exercise.Blocks.Should().BeEmpty();
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
        exercise.Ownership.Origin.Should().Be(DomainEnum.DataOrigin.UserCreated);
        exercise.Ownership.OwnerUserId.Should().Be(owner.Id);
        exercise.Blocks.Should().BeEmpty();
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
        var targetType = DomainEnum.WorkTargetType.Repetitions;

        var movement = Movement.NewBuiltIn(
            movementName,
            movementCategoryId,
            movementDescriptionText
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
        exercise.Ownership.Origin.Should().Be(DomainEnum.DataOrigin.BuiltIn);
        exercise.Ownership.OwnerUserId.Should().BeNull();
        exercise.Blocks.Should().ContainSingle();

        var block = exercise.Blocks.First();

        block.ExerciseId.Should().Be(exercise.Id);
        block.MovementId.Should().Be(movement.Id);
        block.Sequence.Should().Be(1);
        block.Target.Value.Should().Be(value);
        block.Target.TargetType.Should().Be(targetType);
        block.LoadTarget.Type.Should().Be(DomainEnum.LoadTargetType.None);
        block.RestBetweenReps.IsEmpty().Should().BeTrue();
        block.TransitionAfterBlock.IsEmpty().Should().BeTrue();
        block.ExecutionDetails.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void FromMovementUserCreated_ShouldCreateExercise_WithOwner()
    {
        var owner = User.New("Test User");
        var movementName = "  Pull-up  ";
        var movementDescriptionText = "Vertical pulling movement.";
        var movementCategoryId = MovementCategoryId.New();
        var value = 5m;
        var targetType = DomainEnum.WorkTargetType.Repetitions;

        var movement = Movement.NewBuiltIn(
            movementName,
            movementCategoryId,
            movementDescriptionText
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
        exercise.Ownership.Origin.Should().Be(DomainEnum.DataOrigin.UserCreated);
        exercise.Ownership.OwnerUserId.Should().Be(owner.Id);
        exercise.Audit.CreatedBy.Should().Be(owner.Id);
        exercise.Blocks.Should().ContainSingle();
    }

    [Fact]
    public void FromMovementBuiltIn_ShouldCreateExercise_WithProvidedOptionalTargets()
    {
        var owner = User.New("Test User");
        var movementName = "  Weighted Pull-up  ";
        var movementDescriptionText = "Vertical pulling movement with external load.";
        var movementCategoryId = MovementCategoryId.New();
        var value = 5m;
        var targetType = DomainEnum.WorkTargetType.Repetitions;

        var loadTarget = LoadTarget.AddedBodyWeightLoad(
            20m,
            DomainEnum.LoadUnit.Kilogram
        );

        var restBetweenReps = RestTarget.SecondsDuration(5);
        var transitionAfterBlock = RestTarget.SecondsDuration(120);

        var executionDetails = RepExecutionDetails.New(
            eccentricSeconds: 3,
            bottomPauseSeconds: 1,
            concentricSeconds: 1,
            topPauseSeconds: 0,
            eccentricIntent: DomainEnum.RepPhaseExecutionIntent.Controlled,
            bottomIntent: DomainEnum.RepPhaseExecutionIntent.Paused,
            concentricIntent: DomainEnum.RepPhaseExecutionIntent.Explosive,
            topIntent: DomainEnum.RepPhaseExecutionIntent.Strict,
            intent: "Keep the reps clean."
        );

        var movement = Movement.NewBuiltIn(
            movementName,
            movementCategoryId,
            movementDescriptionText
        );

        var exercise = Exercise.FromMovementBuiltIn(
            movement,
            value,
            targetType,
            owner,
            loadTarget,
            restBetweenReps,
            transitionAfterBlock,
            executionDetails
        );

        var block = exercise.Blocks.First();

        block.Target.Value.Should().Be(value);
        block.Target.TargetType.Should().Be(targetType);
        block.LoadTarget.Should().Be(loadTarget);
        block.RestBetweenReps.Should().Be(restBetweenReps);
        block.TransitionAfterBlock.Should().Be(transitionAfterBlock);
        block.ExecutionDetails.Should().Be(executionDetails);
    }

    [Fact]
    public void FromMovementBuiltIn_ShouldThrow_WhenMovementIsNull()
    {
        Movement movement = null!;
        var value = 5m;
        var targetType = DomainEnum.WorkTargetType.Repetitions;

        var act = () => Exercise.FromMovementBuiltIn(
            movement,
            value,
            targetType
        );

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddBlock_ShouldAddBlock_WithNextSequence()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        var firstMovementId = MovementId.New();
        var secondMovementId = MovementId.New();

        exercise.AddBlock(
            firstMovementId,
            5m,
            DomainEnum.WorkTargetType.Repetitions
        );

        exercise.AddBlock(
            secondMovementId,
            10m,
            DomainEnum.WorkTargetType.Repetitions
        );

        exercise.Blocks.Should().HaveCount(2);

        exercise.Blocks.ElementAt(0).MovementId.Should().Be(firstMovementId);
        exercise.Blocks.ElementAt(0).Sequence.Should().Be(1);

        exercise.Blocks.ElementAt(1).MovementId.Should().Be(secondMovementId);
        exercise.Blocks.ElementAt(1).Sequence.Should().Be(2);
    }

    [Fact]
    public void AddBlock_ShouldAddBlock_WithProvidedOptionalTargets()
    {
        var exercise = Exercise.NewBuiltIn(
            "Weighted Pull-up Exercise",
            Description.None(),
            null
        );

        var movementId = MovementId.New();
        var value = 5m;
        var targetType = DomainEnum.WorkTargetType.Repetitions;

        var loadTarget = LoadTarget.AddedBodyWeightLoad(
            20m,
            DomainEnum.LoadUnit.Kilogram
        );

        var restBetweenReps = RestTarget.SecondsDuration(5);
        var transitionAfterBlock = RestTarget.SecondsDuration(120);

        var executionDetails = RepExecutionDetails.New(
            eccentricSeconds: 3,
            concentricSeconds: 1,
            intent: "Controlled eccentric, explosive concentric."
        );

        exercise.AddBlock(
            movementId,
            value,
            targetType,
            loadTarget,
            restBetweenReps,
            transitionAfterBlock,
            executionDetails
        );

        var block = exercise.Blocks.First();

        block.MovementId.Should().Be(movementId);
        block.Sequence.Should().Be(1);
        block.Target.Value.Should().Be(value);
        block.Target.TargetType.Should().Be(targetType);
        block.LoadTarget.Should().Be(loadTarget);
        block.RestBetweenReps.Should().Be(restBetweenReps);
        block.TransitionAfterBlock.Should().Be(transitionAfterBlock);
        block.ExecutionDetails.Should().Be(executionDetails);
    }

    [Fact]
    public void AddBlock_ShouldMarkExerciseAsUpdated()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        var previousUpdatedAt = exercise.Audit.UpdatedAt;

        exercise.AddBlock(
            MovementId.New(),
            5m,
            DomainEnum.WorkTargetType.Repetitions
        );

        exercise.Audit.UpdatedAt.Should().NotBe(previousUpdatedAt);
    }

    [Fact]
    public void AddBlock_ShouldThrow_WhenWorkTargetValueIsNotGreaterThanZero()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        var movementId = MovementId.New();
        var value = 0m;
        var targetType = DomainEnum.WorkTargetType.Repetitions;

        var act = () => exercise.AddBlock(
            movementId,
            value,
            targetType
        );

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RemoveBlock_ShouldRemoveBlock_WhenBlockExists()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        exercise.AddBlock(
            MovementId.New(),
            5m,
            DomainEnum.WorkTargetType.Repetitions
        );

        var blockId = exercise.Blocks.First().Id;

        exercise.RemoveBlock(blockId);

        exercise.Blocks.Should().BeEmpty();
    }

    [Fact]
    public void RemoveBlock_ShouldDoNothing_WhenBlockDoesNotExist()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        var exerciseBlockId = ExerciseBlockId.New();

        exercise.RemoveBlock(exerciseBlockId);

        exercise.Blocks.Should().BeEmpty();
    }

    [Fact]
    public void RemoveBlock_ShouldResequenceRemainingBlocks()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        exercise.AddBlock(
            MovementId.New(),
            5m,
            DomainEnum.WorkTargetType.Repetitions
        );

        exercise.AddBlock(
            MovementId.New(),
            10m,
            DomainEnum.WorkTargetType.Repetitions
        );

        exercise.AddBlock(
            MovementId.New(),
            15m,
            DomainEnum.WorkTargetType.Repetitions
        );

        var secondBlockId = exercise.Blocks.ElementAt(1).Id;

        exercise.RemoveBlock(secondBlockId);

        exercise.Blocks.Should().HaveCount(2);
        exercise.Blocks.ElementAt(0).Sequence.Should().Be(1);
        exercise.Blocks.ElementAt(1).Sequence.Should().Be(2);
    }

    [Fact]
    public void MoveBlock_ShouldMoveBlockAndResequenceBlocks()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        var firstMovementId = MovementId.New();
        var secondMovementId = MovementId.New();
        var thirdMovementId = MovementId.New();

        exercise.AddBlock(
            firstMovementId,
            5m,
            DomainEnum.WorkTargetType.Repetitions
        );

        exercise.AddBlock(
            secondMovementId,
            10m,
            DomainEnum.WorkTargetType.Repetitions
        );

        exercise.AddBlock(
            thirdMovementId,
            15m,
            DomainEnum.WorkTargetType.Repetitions
        );

        var thirdBlockId = exercise.Blocks.ElementAt(2).Id;

        exercise.MoveBlock(
            thirdBlockId,
            1
        );

        exercise.Blocks.ElementAt(0).MovementId.Should().Be(thirdMovementId);
        exercise.Blocks.ElementAt(0).Sequence.Should().Be(1);

        exercise.Blocks.ElementAt(1).Sequence.Should().Be(2);
        exercise.Blocks.ElementAt(2).Sequence.Should().Be(3);
    }

    [Fact]
    public void MoveBlock_ShouldThrow_WhenNewSequenceIsLessThanOne()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        exercise.AddBlock(
            MovementId.New(),
            5m,
            DomainEnum.WorkTargetType.Repetitions
        );

        var blockId = exercise.Blocks.First().Id;

        var act = () => exercise.MoveBlock(
            blockId,
            0
        );

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void MoveBlock_ShouldDoNothing_WhenBlockDoesNotExist()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        exercise.AddBlock(
            MovementId.New(),
            5m,
            DomainEnum.WorkTargetType.Repetitions
        );

        var missingBlockId = ExerciseBlockId.New();

        exercise.MoveBlock(
            missingBlockId,
            1
        );

        exercise.Blocks.Should().ContainSingle();
        exercise.Blocks.First().Sequence.Should().Be(1);
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
    public void ChangeBlockTarget_ShouldChangeTarget_WhenBlockExists()
    {
        var exercise = CreateExerciseWithOneBlock();

        var blockId = exercise.Blocks.First().Id;
        var newValue = 30m;
        var newTargetType = DomainEnum.WorkTargetType.DurationSeconds;

        exercise.ChangeBlockTarget(
            blockId,
            newValue,
            newTargetType
        );

        exercise.Blocks.First().Target.Value.Should().Be(newValue);
        exercise.Blocks.First().Target.TargetType.Should().Be(newTargetType);
    }

    [Fact]
    public void ChangeBlockTarget_ShouldDoNothing_WhenBlockDoesNotExist()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        var missingBlockId = ExerciseBlockId.New();

        exercise.ChangeBlockTarget(
            missingBlockId,
            30m,
            DomainEnum.WorkTargetType.DurationSeconds
        );

        exercise.Blocks.Should().BeEmpty();
    }

    [Fact]
    public void ChangeBlockLoadTarget_ShouldChangeLoadTarget_WhenBlockExists()
    {
        var exercise = CreateExerciseWithOneBlock();
        var blockId = exercise.Blocks.First().Id;

        var loadTarget = LoadTarget.ExternalLoad(
            80m,
            DomainEnum.LoadUnit.Kilogram
        );

        exercise.ChangeBlockLoadTarget(
            blockId,
            loadTarget
        );

        exercise.Blocks.First().LoadTarget.Should().Be(loadTarget);
    }

    [Fact]
    public void ChangeBlockLoadTarget_ShouldDoNothing_WhenBlockDoesNotExist()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        var missingBlockId = ExerciseBlockId.New();

        var loadTarget = LoadTarget.ExternalLoad(
            80m,
            DomainEnum.LoadUnit.Kilogram
        );

        exercise.ChangeBlockLoadTarget(
            missingBlockId,
            loadTarget
        );

        exercise.Blocks.Should().BeEmpty();
    }

    [Fact]
    public void RemoveBlockLoadTarget_ShouldSetLoadTargetToNone_WhenBlockExists()
    {
        var exercise = CreateExerciseWithOneBlock();
        var blockId = exercise.Blocks.First().Id;

        var loadTarget = LoadTarget.ExternalLoad(
            80m,
            DomainEnum.LoadUnit.Kilogram
        );

        exercise.ChangeBlockLoadTarget(
            blockId,
            loadTarget
        );

        exercise.RemoveBlockLoadTarget(blockId);

        exercise.Blocks.First().LoadTarget.Type.Should().Be(DomainEnum.LoadTargetType.None);
        exercise.Blocks.First().LoadTarget.Value.Should().BeNull();
        exercise.Blocks.First().LoadTarget.Unit.Should().BeNull();
    }

    [Fact]
    public void ChangeBlockRestBetweenReps_ShouldChangeRestBetweenReps_WhenBlockExists()
    {
        var exercise = CreateExerciseWithOneBlock();
        var blockId = exercise.Blocks.First().Id;
        var restBetweenReps = RestTarget.SecondsDuration(10);

        exercise.ChangeBlockRestBetweenReps(
            blockId,
            restBetweenReps
        );

        exercise.Blocks.First().RestBetweenReps.Should().Be(restBetweenReps);
    }

    [Fact]
    public void RemoveBlockRestBetweenReps_ShouldSetRestBetweenRepsToNone_WhenBlockExists()
    {
        var exercise = CreateExerciseWithOneBlock();
        var blockId = exercise.Blocks.First().Id;
        var restBetweenReps = RestTarget.SecondsDuration(10);

        exercise.ChangeBlockRestBetweenReps(
            blockId,
            restBetweenReps
        );

        exercise.RemoveBlockRestBetweenReps(blockId);

        exercise.Blocks.First().RestBetweenReps.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void ChangeBlockTransitionAfterBlock_ShouldChangeTransitionAfterBlock_WhenBlockExists()
    {
        var exercise = CreateExerciseWithOneBlock();
        var blockId = exercise.Blocks.First().Id;
        var transitionAfterBlock = RestTarget.SecondsDuration(120);

        exercise.ChangeBlockTransitionAfterBlock(
            blockId,
            transitionAfterBlock
        );

        exercise.Blocks.First().TransitionAfterBlock.Should().Be(transitionAfterBlock);
    }

    [Fact]
    public void RemoveBlockTransitionAfterBlock_ShouldSetTransitionAfterBlockToNone_WhenBlockExists()
    {
        var exercise = CreateExerciseWithOneBlock();
        var blockId = exercise.Blocks.First().Id;
        var transitionAfterBlock = RestTarget.SecondsDuration(120);

        exercise.ChangeBlockTransitionAfterBlock(
            blockId,
            transitionAfterBlock
        );

        exercise.RemoveBlockTransitionAfterBlock(blockId);

        exercise.Blocks.First().TransitionAfterBlock.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void ChangeBlockExecutionDetails_ShouldChangeExecutionDetails_WhenBlockExists()
    {
        var exercise = CreateExerciseWithOneBlock();
        var blockId = exercise.Blocks.First().Id;

        var executionDetails = RepExecutionDetails.New(
            eccentricSeconds: 3,
            concentricSeconds: 1,
            intent: "Controlled eccentric."
        );

        exercise.ChangeBlockExecutionDetails(
            blockId,
            executionDetails
        );

        exercise.Blocks.First().ExecutionDetails.Should().Be(executionDetails);
    }

    [Fact]
    public void RemoveBlockExecutionDetails_ShouldSetExecutionDetailsToEmpty_WhenBlockExists()
    {
        var exercise = CreateExerciseWithOneBlock();
        var blockId = exercise.Blocks.First().Id;

        var executionDetails = RepExecutionDetails.New(
            eccentricSeconds: 3,
            concentricSeconds: 1,
            intent: "Controlled eccentric."
        );

        exercise.ChangeBlockExecutionDetails(
            blockId,
            executionDetails
        );

        exercise.RemoveBlockExecutionDetails(blockId);

        exercise.Blocks.First().ExecutionDetails.IsEmpty().Should().BeTrue();
    }

    private static Exercise CreateExerciseWithOneBlock()
    {
        var exercise = Exercise.NewBuiltIn(
            "Pull-up Exercise",
            Description.None(),
            null
        );

        exercise.AddBlock(
            MovementId.New(),
            5m,
            DomainEnum.WorkTargetType.Repetitions
        );

        return exercise;
    }
}