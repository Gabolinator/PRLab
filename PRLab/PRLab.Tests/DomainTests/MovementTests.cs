using FluentAssertions;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Interface;
using PRLab.Domain.Model.Value.Enum.Anatomy;
using PRLab.Domain.Model.Value.Enum.Movement;
using PRLab.Domain.Model.Value.Enum.Prescription;
using PRLab.Domain.Model.Value.Enum.Prescription.Work;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities;

namespace PRLab.Tests.DomainTests;

public sealed class MovementTests
{
    private static Movement CreateBuiltInMovement(
        string name = "Back Squat",
        MovementCategoryId? movementCategoryId = null,
        string? description = null,
        WorkTargetType defaultWorkTargetType = WorkTargetType.Repetitions,
        MovementLaterality laterality = MovementLaterality.Bilateral,
        IReadOnlyCollection<WorkTargetType>? allowedWorkTargetTypes = null)
    {
        return Movement.NewBuiltIn(
            name,
            movementCategoryId ?? MovementCategoryId.New(),
            description,
            defaultWorkTargetType,
            laterality,
            allowedWorkTargetTypes);
    }

    [Fact]
    public void NewBuiltIn_ShouldCreateMovement_WithNormalizedNameAndNameKey()
    {
        var name = "  Back Squat  ";
        var movementCategoryId = MovementCategoryId.New();
        var descriptionText = "A squat variation performed with a barbell.";

        var movement = Movement.NewBuiltIn(
            name,
            movementCategoryId,
            descriptionText,
            WorkTargetType.Repetitions,
            MovementLaterality.Bilateral);

        movement.Name.Should().Be(FormatingUtilities.NormalizeName(name));
        movement.NameKey.Should().Be(FormatingUtilities.NormalizeNameKey(name));
        movement.MovementCategoryId.Should().Be(movementCategoryId);
        movement.Laterality.Should().Be(MovementLaterality.Bilateral);
        movement.Description.GetContent().Should().Be(descriptionText);
        movement.DefaultWorkTargetType.Should().Be(WorkTargetType.Repetitions);
        movement.AllowedWorkTargets.Should().ContainSingle();
        movement.AllowedWorkTargets.First().TargetType.Should().Be(WorkTargetType.Repetitions);
        movement.Audit.Should().NotBeNull();
        movement.Audit.IsDeleted.Should().BeFalse();
        movement.Ownership.Should().NotBeNull();
        movement.Ownership.Origin.Should().Be(DataOrigin.BuiltIn);
        movement.Ownership.OwnerUserId.Should().BeNull();
        movement.Muscles.Should().BeEmpty();
        movement.EquipmentRequirements.Should().BeEmpty();
        movement.Variants.Should().BeEmpty();
        movement.Patterns.Should().BeEmpty();
        movement.PrimaryPattern.Should().BeNull();
    }

    [Fact]
    public void NewBuiltIn_ShouldCreateMovement_WithUnilateralLaterality()
    {
        var movement = Movement.NewBuiltIn(
            "Single Arm Row",
            MovementCategoryId.New(),
            "Single-side pulling movement.",
            WorkTargetType.Repetitions,
            MovementLaterality.Unilateral);

        movement.Laterality.Should().Be(MovementLaterality.Unilateral);
    }

    [Fact]
    public void NewUserCreated_ShouldCreateMovement_WithOwner()
    {
        var owner = User.New("Test User");
        var name = "  Custom Movement  ";
        var movementCategoryId = MovementCategoryId.New();
        var descriptionText = "User-created movement.";

        var movement = Movement.NewUserCreated(
            name,
            movementCategoryId,
            descriptionText,
            owner,
            WorkTargetType.Repetitions,
            MovementLaterality.Bilateral);

        movement.Name.Should().Be(FormatingUtilities.NormalizeName(name));
        movement.NameKey.Should().Be(FormatingUtilities.NormalizeNameKey(name));
        movement.MovementCategoryId.Should().Be(movementCategoryId);
        movement.Laterality.Should().Be(MovementLaterality.Bilateral);
        movement.Description.GetContent().Should().Be(descriptionText);
        movement.DefaultWorkTargetType.Should().Be(WorkTargetType.Repetitions);
        movement.AllowedWorkTargets.Should().ContainSingle();
        movement.AllowedWorkTargets.First().TargetType.Should().Be(WorkTargetType.Repetitions);
        movement.Audit.Should().NotBeNull();
        movement.Audit.CreatedBy.Should().Be(owner.Id);
        movement.Ownership.Should().NotBeNull();
        movement.Ownership.Origin.Should().Be(DataOrigin.UserCreated);
        movement.Ownership.OwnerUserId.Should().Be(owner.Id);
    }

    [Fact]
    public void NewBuiltIn_ShouldThrow_WhenNameIsEmpty()
    {
        var name = "   ";
        var movementCategoryId = MovementCategoryId.New();
        var descriptionText = "Invalid movement.";

        var act = () => Movement.NewBuiltIn(
            name,
            movementCategoryId,
            descriptionText,
            WorkTargetType.Repetitions,
            MovementLaterality.Bilateral);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NewBuiltIn_ShouldCreateAllowedWorkTarget_WithDefaultTarget_WhenAllowedTargetsAreEmpty()
    {
        var movement = Movement.NewBuiltIn(
            "Push Up",
            MovementCategoryId.New(),
            null,
            WorkTargetType.Repetitions,
            MovementLaterality.Bilateral);

        movement.DefaultWorkTargetType.Should().Be(WorkTargetType.Repetitions);
        movement.AllowedWorkTargets.Should().ContainSingle();
        movement.AllowedWorkTargets.First().TargetType.Should().Be(WorkTargetType.Repetitions);
    }

    [Fact]
    public void NewBuiltIn_ShouldIncludeDefaultWorkTarget_InAllowedTargets_WhenMissingFromAllowedTargets()
    {
        var movement = Movement.NewBuiltIn(
            "Running",
            MovementCategoryId.New(),
            null,
            WorkTargetType.DistanceMeters,
            MovementLaterality.Bilateral,
            [
                WorkTargetType.DurationSeconds,
                WorkTargetType.Calories
            ]);

        movement.DefaultWorkTargetType.Should().Be(WorkTargetType.DistanceMeters);
        movement.AllowedWorkTargets
            .Select(allowedWorkTarget => allowedWorkTarget.TargetType)
            .Should()
            .BeEquivalentTo(
            [
                WorkTargetType.DistanceMeters,
                WorkTargetType.DurationSeconds,
                WorkTargetType.Calories
            ]);
    }

    [Fact]
    public void SetDefaultWorkTargetType_ShouldSetDefaultAndAddAllowedTarget_WhenTargetIsNotAlreadyAllowed()
    {
        var movement = CreateBuiltInMovement(
            name: "Running",
            defaultWorkTargetType: WorkTargetType.DurationSeconds);

        movement.SetDefaultWorkTargetType(WorkTargetType.DistanceMeters);

        movement.DefaultWorkTargetType.Should().Be(WorkTargetType.DistanceMeters);
        movement.AllowedWorkTargets
            .Select(allowedWorkTarget => allowedWorkTarget.TargetType)
            .Should()
            .Contain(WorkTargetType.DistanceMeters);
    }

    [Fact]
    public void AddAllowedWorkTargetType_ShouldAddTarget_WhenNotAlreadyAllowed()
    {
        var movement = CreateBuiltInMovement("Push Up");

        movement.AddAllowedWorkTargetType(WorkTargetType.TimeUnderTensionSeconds);

        movement.AllowedWorkTargets
            .Select(allowedWorkTarget => allowedWorkTarget.TargetType)
            .Should()
            .BeEquivalentTo(
            [
                WorkTargetType.Repetitions,
                WorkTargetType.TimeUnderTensionSeconds
            ]);
    }

    [Fact]
    public void AddAllowedWorkTargetType_ShouldNotAddDuplicate_WhenTargetAlreadyAllowed()
    {
        var movement = CreateBuiltInMovement("Push Up");

        movement.AddAllowedWorkTargetType(WorkTargetType.Repetitions);

        movement.AllowedWorkTargets.Should().ContainSingle();
    }

    [Fact]
    public void RemoveAllowedWorkTargetType_ShouldRemoveTarget_WhenTargetIsNotDefault()
    {
        var movement = CreateBuiltInMovement(
            name: "Running",
            defaultWorkTargetType: WorkTargetType.DistanceMeters,
            allowedWorkTargetTypes:
            [
                WorkTargetType.DistanceMeters,
                WorkTargetType.DurationSeconds
            ]);

        movement.RemoveAllowedWorkTargetType(WorkTargetType.DurationSeconds);

        movement.AllowedWorkTargets
            .Select(allowedWorkTarget => allowedWorkTarget.TargetType)
            .Should()
            .ContainSingle()
            .Which
            .Should()
            .Be(WorkTargetType.DistanceMeters);
    }

    [Fact]
    public void RemoveAllowedWorkTargetType_ShouldThrow_WhenTargetIsDefault()
    {
        var movement = CreateBuiltInMovement(
            name: "Running",
            defaultWorkTargetType: WorkTargetType.DistanceMeters,
            allowedWorkTargetTypes:
            [
                WorkTargetType.DistanceMeters,
                WorkTargetType.DurationSeconds
            ]);

        var act = () => movement.RemoveAllowedWorkTargetType(WorkTargetType.DistanceMeters);

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Rename_ShouldUpdateNameAndNameKey()
    {
        var initialName = "Squat";
        var newName = "  Back Squat  ";
        var movement = CreateBuiltInMovement(initialName);

        movement.Rename(newName);

        movement.Name.Should().Be(FormatingUtilities.NormalizeName(newName));
        movement.NameKey.Should().Be(FormatingUtilities.NormalizeNameKey(newName));
    }

    [Fact]
    public void Rename_ShouldMarkMovementAsUpdated()
    {
        var initialName = "Squat";
        var newName = "Back Squat";
        var movement = CreateBuiltInMovement(initialName);

        var previousUpdatedAt = movement.Audit.UpdatedAt;

        movement.Rename(newName);

        movement.Audit.UpdatedAt.Should().NotBe(previousUpdatedAt);
    }

    [Fact]
    public void ChangeCategory_ShouldUpdateMovementCategoryId()
    {
        var initialMovementCategoryId = MovementCategoryId.New();
        var newMovementCategoryId = MovementCategoryId.New();

        var movement = CreateBuiltInMovement(
            name: "Squat",
            movementCategoryId: initialMovementCategoryId);

        movement.ChangeCategory(newMovementCategoryId);

        movement.MovementCategoryId.Should().Be(newMovementCategoryId);
    }

    [Fact]
    public void ChangeDescription_ShouldChangeDescriptionContent()
    {
        var initialDescriptionText = "Initial description.";
        var newDescriptionText = "Updated description.";
        var language = LocalizationHelper.Language.EN;

        var movement = CreateBuiltInMovement(
            name: "Squat",
            description: initialDescriptionText);

        movement.ChangeDescription(
            newDescriptionText,
            language);

        movement.Description.GetContent(language).Should().Be(newDescriptionText);
    }

    [Fact]
    public void RemoveDescription_ShouldRemoveDescriptionContent()
    {
        var descriptionText = "Initial description.";
        var language = LocalizationHelper.Language.EN;

        var movement = CreateBuiltInMovement(
            name: "Squat",
            description: descriptionText);

        movement.RemoveDescription(language);

        movement.Description.GetContent(language).Should().BeNull();
    }

    [Fact]
    public void AddPrimaryMuscle_ShouldAddMuscleWithPrimaryRole()
    {
        var muscleId = MuscleId.New();

        var movement = CreateBuiltInMovement("Bench Press");

        movement.AddPrimaryMuscle(muscleId);

        movement.Muscles.Should().ContainSingle();
        movement.Muscles.First().MovementId.Should().Be(movement.Id);
        movement.Muscles.First().MuscleId.Should().Be(muscleId);
        movement.Muscles.First().Role.Should().Be(MuscleRole.Primary);
    }

    [Fact]
    public void AddSecondaryMuscle_ShouldAddMuscleWithSecondaryRole()
    {
        var muscleId = MuscleId.New();

        var movement = CreateBuiltInMovement("Bench Press");

        movement.AddSecondaryMuscle(muscleId);

        movement.Muscles.Should().ContainSingle();
        movement.Muscles.First().MovementId.Should().Be(movement.Id);
        movement.Muscles.First().MuscleId.Should().Be(muscleId);
        movement.Muscles.First().Role.Should().Be(MuscleRole.Secondary);
    }

    [Fact]
    public void AddMuscle_ShouldNotAddDuplicate_WhenMuscleAlreadyExists()
    {
        var muscleId = MuscleId.New();

        var movement = CreateBuiltInMovement("Bench Press");

        movement.AddMuscle(
            muscleId,
            MuscleRole.Primary);

        movement.AddMuscle(
            muscleId,
            MuscleRole.Secondary);

        movement.Muscles.Should().ContainSingle();
        movement.Muscles.First().Role.Should().Be(MuscleRole.Primary);
    }

    [Fact]
    public void ChangeMuscleRole_ShouldChangeRole_WhenMuscleExists()
    {
        var muscleId = MuscleId.New();

        var movement = CreateBuiltInMovement("Bench Press");

        movement.AddMuscle(
            muscleId,
            MuscleRole.Secondary);

        movement.ChangeMuscleRole(
            muscleId,
            MuscleRole.Primary);

        movement.Muscles.Should().ContainSingle();
        movement.Muscles.First().Role.Should().Be(MuscleRole.Primary);
    }

    [Fact]
    public void ChangeMuscleRole_ShouldDoNothing_WhenMuscleDoesNotExist()
    {
        var muscleId = MuscleId.New();

        var movement = CreateBuiltInMovement("Bench Press");

        movement.ChangeMuscleRole(
            muscleId,
            MuscleRole.Primary);

        movement.Muscles.Should().BeEmpty();
    }

    [Fact]
    public void RemoveMuscle_ShouldRemoveMuscle_WhenMuscleExists()
    {
        var muscleId = MuscleId.New();

        var movement = CreateBuiltInMovement("Bench Press");

        movement.AddPrimaryMuscle(muscleId);

        movement.RemoveMuscle(muscleId);

        movement.Muscles.Should().BeEmpty();
    }

    [Fact]
    public void RemoveMuscle_ShouldDoNothing_WhenMuscleDoesNotExist()
    {
        var muscleId = MuscleId.New();

        var movement = CreateBuiltInMovement("Bench Press");

        movement.RemoveMuscle(muscleId);

        movement.Muscles.Should().BeEmpty();
    }

    [Fact]
    public void AddRequiredEquipmentOption_ShouldAddRequiredEquipmentOption_WhenNotAlreadyAdded()
    {
        var equipmentId = EquipmentId.New();
        var groupKey = "load";

        var movement = CreateBuiltInMovement("Back Squat");

        movement.AddRequiredEquipmentOption(
            equipmentId,
            groupKey);

        movement.EquipmentRequirements.Should().ContainSingle();
        movement.EquipmentRequirements.First().MovementId.Should().Be(movement.Id);
        movement.EquipmentRequirements.First().EquipmentId.Should().Be(equipmentId);
        movement.EquipmentRequirements.First().GroupKey.Should().Be(groupKey);
        movement.EquipmentRequirements.First().Kind.Should().Be(EquipmentRequirementKind.RequiredGroup);
    }

    [Fact]
    public void AddRequiredEquipmentOption_ShouldNotAddDuplicate_WhenSameEquipmentRequirementAlreadyExists()
    {
        var equipmentId = EquipmentId.New();
        var groupKey = "load";

        var movement = CreateBuiltInMovement("Back Squat");

        movement.AddRequiredEquipmentOption(
            equipmentId,
            groupKey);

        movement.AddRequiredEquipmentOption(
            equipmentId,
            groupKey);

        movement.EquipmentRequirements.Should().ContainSingle();
    }

    [Fact]
    public void AddRequiredEquipmentOption_ShouldAddSameEquipment_WhenGroupKeyIsDifferent()
    {
        var equipmentId = EquipmentId.New();
        var firstGroupKey = "load";
        var secondGroupKey = "support";

        var movement = CreateBuiltInMovement("Back Squat");

        movement.AddRequiredEquipmentOption(
            equipmentId,
            firstGroupKey);

        movement.AddRequiredEquipmentOption(
            equipmentId,
            secondGroupKey);

        movement.EquipmentRequirements.Should().HaveCount(2);
        movement.EquipmentRequirements.Should().Contain(requirement =>
            requirement.EquipmentId == equipmentId
            && requirement.GroupKey == firstGroupKey
            && requirement.Kind == EquipmentRequirementKind.RequiredGroup);
        movement.EquipmentRequirements.Should().Contain(requirement =>
            requirement.EquipmentId == equipmentId
            && requirement.GroupKey == secondGroupKey
            && requirement.Kind == EquipmentRequirementKind.RequiredGroup);
    }

    [Fact]
    public void AddOptionalEquipment_ShouldAddOptionalEquipment_WhenNotAlreadyAdded()
    {
        var equipmentId = EquipmentId.New();
        var groupKey = "assistance";

        var movement = CreateBuiltInMovement("Pull Up");

        movement.AddOptionalEquipment(
            equipmentId,
            groupKey);

        movement.EquipmentRequirements.Should().ContainSingle();
        movement.EquipmentRequirements.First().MovementId.Should().Be(movement.Id);
        movement.EquipmentRequirements.First().EquipmentId.Should().Be(equipmentId);
        movement.EquipmentRequirements.First().GroupKey.Should().Be(groupKey);
        movement.EquipmentRequirements.First().Kind.Should().Be(EquipmentRequirementKind.Optional);
    }

    [Fact]
    public void RemoveEquipmentRequirement_ShouldRemoveEquipmentRequirement_WhenRequirementExists()
    {
        var equipmentId = EquipmentId.New();
        var groupKey = "load";

        var movement = CreateBuiltInMovement("Back Squat");

        movement.AddRequiredEquipmentOption(
            equipmentId,
            groupKey);

        movement.RemoveEquipmentRequirement(
            equipmentId,
            groupKey,
            EquipmentRequirementKind.RequiredGroup);

        movement.EquipmentRequirements.Should().BeEmpty();
    }

    [Fact]
    public void RemoveEquipmentRequirement_ShouldDoNothing_WhenRequirementDoesNotExist()
    {
        var equipmentId = EquipmentId.New();
        var groupKey = "load";

        var movement = CreateBuiltInMovement("Back Squat");

        movement.RemoveEquipmentRequirement(
            equipmentId,
            groupKey,
            EquipmentRequirementKind.RequiredGroup);

        movement.EquipmentRequirements.Should().BeEmpty();
    }

    [Fact]
    public void MakeVariantOf_ShouldSetVariantParent()
    {
        var parentMovementId = MovementId.New();

        var movement = CreateBuiltInMovement("Front Squat");

        movement.MakeVariantOf(parentMovementId);

        movement.VariantOfId.Should().Be(parentMovementId);
    }

    [Fact]
    public void MakeVariantOf_ShouldThrow_WhenMovementIsOwnVariantParent()
    {
        var movement = CreateBuiltInMovement("Front Squat");

        var act = () => movement.MakeVariantOf(movement.Id);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RemoveVariantParent_ShouldClearVariantParent_WhenParentExists()
    {
        var parentMovementId = MovementId.New();

        var movement = CreateBuiltInMovement("Front Squat");

        movement.MakeVariantOf(parentMovementId);

        movement.RemoveVariantParent();

        movement.VariantOfId.Should().BeNull();
        movement.VariantOf.Should().BeNull();
    }

    [Fact]
    public void RemoveVariantParent_ShouldDoNothing_WhenParentDoesNotExist()
    {
        var movement = CreateBuiltInMovement("Front Squat");

        movement.RemoveVariantParent();

        movement.VariantOfId.Should().BeNull();
        movement.VariantOf.Should().BeNull();
    }

    [Fact]
    public void AddVariant_ShouldAddVariantAndSetVariantParent()
    {
        var movement = CreateBuiltInMovement("Squat");

        var variant = CreateBuiltInMovement("Front Squat");

        movement.AddVariant(variant);

        movement.Variants.Should().ContainSingle();
        movement.Variants.First().Id.Should().Be(variant.Id);
        variant.VariantOfId.Should().Be(movement.Id);
    }

    [Fact]
    public void AddVariant_ShouldNotAddDuplicate_WhenVariantAlreadyExists()
    {
        var movement = CreateBuiltInMovement("Squat");

        var variant = CreateBuiltInMovement("Front Squat");

        movement.AddVariant(variant);
        movement.AddVariant(variant);

        movement.Variants.Should().ContainSingle();
    }

    [Fact]
    public void AddVariant_ShouldThrow_WhenMovementIsOwnVariant()
    {
        var movement = CreateBuiltInMovement("Squat");

        var act = () => movement.AddVariant(movement);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RemoveVariant_ShouldRemoveVariantAndClearVariantParent()
    {
        var movement = CreateBuiltInMovement("Squat");

        var variant = CreateBuiltInMovement("Front Squat");

        movement.AddVariant(variant);

        movement.RemoveVariant(variant.Id);

        movement.Variants.Should().BeEmpty();
        variant.VariantOfId.Should().BeNull();
        variant.VariantOf.Should().BeNull();
    }

    [Fact]
    public void RemoveVariant_ShouldDoNothing_WhenVariantDoesNotExist()
    {
        var movement = CreateBuiltInMovement("Squat");

        var variantId = MovementId.New();

        movement.RemoveVariant(variantId);

        movement.Variants.Should().BeEmpty();
    }

    [Fact]
    public void AddPattern_ShouldAddPattern_WhenPatternDoesNotExist()
    {
        var pattern = MovementPattern.Squat;

        var movement = CreateBuiltInMovement("Back Squat");

        movement.AddPattern(pattern);

        movement.Patterns.Should().ContainSingle();
        movement.Patterns.First().MovementId.Should().Be(movement.Id);
        movement.Patterns.First().Pattern.Should().Be(pattern);
    }

    [Fact]
    public void AddPattern_ShouldNotAddDuplicate_WhenPatternAlreadyExists()
    {
        var pattern = MovementPattern.Squat;

        var movement = CreateBuiltInMovement("Back Squat");

        movement.AddPattern(pattern);
        movement.AddPattern(pattern);

        movement.Patterns.Should().ContainSingle();
    }

    [Fact]
    public void AddPattern_ShouldThrow_WhenPatternIsComplex()
    {
        var pattern = MovementPattern.Complex;

        var movement = CreateBuiltInMovement("Clean And Jerk");

        var act = () => movement.AddPattern(pattern);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RemovePattern_ShouldRemovePattern_WhenPatternExists()
    {
        var pattern = MovementPattern.Squat;

        var movement = CreateBuiltInMovement("Back Squat");

        movement.AddPattern(pattern);

        movement.RemovePattern(pattern);

        movement.Patterns.Should().BeEmpty();
    }

    [Fact]
    public void RemovePattern_ShouldDoNothing_WhenPatternDoesNotExist()
    {
        var pattern = MovementPattern.Squat;

        var movement = CreateBuiltInMovement("Back Squat");

        movement.RemovePattern(pattern);

        movement.Patterns.Should().BeEmpty();
    }

    [Fact]
    public void SetPrimaryPattern_ShouldSetPrimaryPattern_AndAddPatternTag_WhenPatternIsNotComplex()
    {
        var pattern = MovementPattern.Squat;

        var movement = CreateBuiltInMovement("Back Squat");

        movement.SetPrimaryPattern(pattern);

        movement.PrimaryPattern.Should().Be(pattern);
        movement.Patterns.Should().ContainSingle();
        movement.Patterns.First().Pattern.Should().Be(pattern);
    }

    [Fact]
    public void SetPrimaryPattern_ShouldSetPrimaryPattern_WithoutAddingPatternTag_WhenPatternIsComplex()
    {
        var pattern = MovementPattern.Complex;

        var movement = CreateBuiltInMovement("Clean And Jerk");

        movement.SetPrimaryPattern(pattern);

        movement.PrimaryPattern.Should().Be(pattern);
        movement.Patterns.Should().BeEmpty();
    }

    [Fact]
    public void ClearPrimaryPattern_ShouldClearPrimaryPattern_WhenPrimaryPatternExists()
    {
        var pattern = MovementPattern.Squat;

        var movement = CreateBuiltInMovement("Back Squat");

        movement.SetPrimaryPattern(pattern);

        movement.ClearPrimaryPattern();

        movement.PrimaryPattern.Should().BeNull();
    }

    [Fact]
    public void ClearPrimaryPattern_ShouldDoNothing_WhenPrimaryPatternDoesNotExist()
    {
        var movement = CreateBuiltInMovement("Back Squat");

        movement.ClearPrimaryPattern();

        movement.PrimaryPattern.Should().BeNull();
    }

    [Fact]
    public void AutoResolvePrimaryPattern_ShouldSetPrimaryPatternToNull_WhenNoPatternExists()
    {
        var movement = CreateBuiltInMovement("Back Squat");

        movement.AutoResolvePrimaryPattern();

        movement.PrimaryPattern.Should().BeNull();
    }

    [Fact]
    public void AutoResolvePrimaryPattern_ShouldSetPrimaryPatternToOnlyPattern_WhenOnePatternExists()
    {
        var pattern = MovementPattern.Squat;

        var movement = CreateBuiltInMovement("Back Squat");

        movement.AddPattern(pattern);

        movement.AutoResolvePrimaryPattern();

        movement.PrimaryPattern.Should().Be(pattern);
    }

    [Fact]
    public void AutoResolvePrimaryPattern_ShouldSetPrimaryPatternToComplex_WhenMultiplePatternsExist()
    {
        var firstPattern = MovementPattern.Squat;
        var secondPattern = MovementPattern.Hinge;

        var movement = CreateBuiltInMovement("Thruster");

        movement.AddPattern(firstPattern);
        movement.AddPattern(secondPattern);

        movement.AutoResolvePrimaryPattern();

        movement.PrimaryPattern.Should().Be(MovementPattern.Complex);
    }

    [Fact]
    public void RemovePattern_ShouldResolvePrimaryPattern_WhenRemovedPatternWasPrimaryPattern()
    {
        var firstPattern = MovementPattern.Squat;
        var secondPattern = MovementPattern.Hinge;

        var movement = CreateBuiltInMovement("Thruster");

        movement.AddPattern(firstPattern);
        movement.AddPattern(secondPattern);
        movement.SetPrimaryPattern(firstPattern);

        movement.RemovePattern(firstPattern);

        movement.PrimaryPattern.Should().Be(secondPattern);
    }

    [Fact]
    public void MarkDeleted_ShouldMarkMovementAsDeleted_WhenCalledThroughAuditedInterface()
    {
        var movement = CreateBuiltInMovement("Back Squat");

        ((IAudited)movement).MarkDeleted();

        movement.Audit.IsDeleted.Should().BeTrue();
    }
}