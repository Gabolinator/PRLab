using FluentAssertions;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Interface;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value.Enum.Anatomy;
using PRLab.Domain.Value.Enum.Movement;
using PRLab.Domain.Value.Enum.System;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Tests.DomainTests;

public sealed class MovementTests
{
    [Fact]
    public void NewBuiltIn_ShouldCreateMovement_WithNormalizedNameAndNameKey()
    {
        var name = "  Back Squat  ";
        var movementCategoryId = MovementCategoryId.New();
        var descriptionText = "A squat variation performed with a barbell.";

        var movement = Movement.NewBuiltIn(
            name,
            movementCategoryId,
            descriptionText
        );

        movement.Name.Should().Be(FormatingUtilities.NormalizeName(name));
        movement.NameKey.Should().Be(FormatingUtilities.NormalizeNameKey(name));
        movement.MovementCategoryId.Should().Be(movementCategoryId);
        movement.Description.GetContent().Should().Be(descriptionText);
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
            owner
        );

        movement.Name.Should().Be(FormatingUtilities.NormalizeName(name));
        movement.NameKey.Should().Be(FormatingUtilities.NormalizeNameKey(name));
        movement.MovementCategoryId.Should().Be(movementCategoryId);
        movement.Description.GetContent().Should().Be(descriptionText);
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
            descriptionText
        );

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Rename_ShouldUpdateNameAndNameKey()
    {
        var initialName = "Squat";
        var newName = "  Back Squat  ";
        var movement = Movement.NewBuiltIn(
            initialName,
            MovementCategoryId.New(),
            null
        );

        movement.Rename(newName);

        movement.Name.Should().Be(FormatingUtilities.NormalizeName(newName));
        movement.NameKey.Should().Be(FormatingUtilities.NormalizeNameKey(newName));
    }

    [Fact]
    public void Rename_ShouldMarkMovementAsUpdated()
    {
        var initialName = "Squat";
        var newName = "Back Squat";
        var movement = Movement.NewBuiltIn(
            initialName,
            MovementCategoryId.New(),
            null
        );

        var previousUpdatedAt = movement.Audit.UpdatedAt;

        movement.Rename(newName);

        movement.Audit.UpdatedAt.Should().NotBe(previousUpdatedAt);
    }

    [Fact]
    public void ChangeCategory_ShouldUpdateMovementCategoryId()
    {
        var initialMovementCategoryId = MovementCategoryId.New();
        var newMovementCategoryId = MovementCategoryId.New();

        var movement = Movement.NewBuiltIn(
            "Squat",
            initialMovementCategoryId,
            null
        );

        movement.ChangeCategory(newMovementCategoryId);

        movement.MovementCategoryId.Should().Be(newMovementCategoryId);
    }

    [Fact]
    public void ChangeDescription_ShouldChangeDescriptionContent()
    {
        var initialDescriptionText = "Initial description.";
        var newDescriptionText = "Updated description.";
        var language = LocalizationHelper.Language.EN;

        var movement = Movement.NewBuiltIn(
            "Squat",
            MovementCategoryId.New(),
            initialDescriptionText
        );

        movement.ChangeDescription(
            newDescriptionText,
            language
        );

        movement.Description.GetContent(language).Should().Be(newDescriptionText);
    }

    [Fact]
    public void RemoveDescription_ShouldRemoveDescriptionContent()
    {
        var descriptionText = "Initial description.";
        var language = LocalizationHelper.Language.EN;

        var movement = Movement.NewBuiltIn(
            "Squat",
            MovementCategoryId.New(),
            descriptionText
        );

        movement.RemoveDescription(language);

        movement.Description.GetContent(language).Should().BeNull();
    }

    [Fact]
    public void AddPrimaryMuscle_ShouldAddMuscleWithPrimaryRole()
    {
        var muscleId = MuscleId.New();

        var movement = Movement.NewBuiltIn(
            "Bench Press",
            MovementCategoryId.New(),
            null
        );

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

        var movement = Movement.NewBuiltIn(
            "Bench Press",
            MovementCategoryId.New(),
            null
        );

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

        var movement = Movement.NewBuiltIn(
            "Bench Press",
            MovementCategoryId.New(),
            null
        );

        movement.AddMuscle(
            muscleId,
            MuscleRole.Primary
        );

        movement.AddMuscle(
            muscleId,
            MuscleRole.Secondary
        );

        movement.Muscles.Should().ContainSingle();
        movement.Muscles.First().Role.Should().Be(MuscleRole.Primary);
    }

    [Fact]
    public void ChangeMuscleRole_ShouldChangeRole_WhenMuscleExists()
    {
        var muscleId = MuscleId.New();

        var movement = Movement.NewBuiltIn(
            "Bench Press",
            MovementCategoryId.New(),
            null
        );

        movement.AddMuscle(
            muscleId,
            MuscleRole.Secondary
        );

        movement.ChangeMuscleRole(
            muscleId,
            MuscleRole.Primary
        );

        movement.Muscles.Should().ContainSingle();
        movement.Muscles.First().Role.Should().Be(MuscleRole.Primary);
    }

    [Fact]
    public void ChangeMuscleRole_ShouldDoNothing_WhenMuscleDoesNotExist()
    {
        var muscleId = MuscleId.New();

        var movement = Movement.NewBuiltIn(
            "Bench Press",
            MovementCategoryId.New(),
            null
        );

        movement.ChangeMuscleRole(
            muscleId,
            MuscleRole.Primary
        );

        movement.Muscles.Should().BeEmpty();
    }

    [Fact]
    public void RemoveMuscle_ShouldRemoveMuscle_WhenMuscleExists()
    {
        var muscleId = MuscleId.New();

        var movement = Movement.NewBuiltIn(
            "Bench Press",
            MovementCategoryId.New(),
            null
        );

        movement.AddPrimaryMuscle(muscleId);

        movement.RemoveMuscle(muscleId);

        movement.Muscles.Should().BeEmpty();
    }

    [Fact]
    public void RemoveMuscle_ShouldDoNothing_WhenMuscleDoesNotExist()
    {
        var muscleId = MuscleId.New();

        var movement = Movement.NewBuiltIn(
            "Bench Press",
            MovementCategoryId.New(),
            null
        );

        movement.RemoveMuscle(muscleId);

        movement.Muscles.Should().BeEmpty();
    }

    [Fact]
public void AddRequiredEquipmentOption_ShouldAddRequiredEquipmentOption_WhenNotAlreadyAdded()
{
    var equipmentId = EquipmentId.New();
    var groupKey = "load";

    var movement = Movement.NewBuiltIn(
        "Back Squat",
        MovementCategoryId.New(),
        null
    );

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

    var movement = Movement.NewBuiltIn(
        "Back Squat",
        MovementCategoryId.New(),
        null
    );

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

    var movement = Movement.NewBuiltIn(
        "Back Squat",
        MovementCategoryId.New(),
        null
    );

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

    var movement = Movement.NewBuiltIn(
        "Pull Up",
        MovementCategoryId.New(),
        null
    );

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

    var movement = Movement.NewBuiltIn(
        "Back Squat",
        MovementCategoryId.New(),
        null
    );

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

    var movement = Movement.NewBuiltIn(
        "Back Squat",
        MovementCategoryId.New(),
        null
    );

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

        var movement = Movement.NewBuiltIn(
            "Front Squat",
            MovementCategoryId.New(),
            null
        );

        movement.MakeVariantOf(parentMovementId);

        movement.VariantOfId.Should().Be(parentMovementId);
    }

    [Fact]
    public void MakeVariantOf_ShouldThrow_WhenMovementIsOwnVariantParent()
    {
        var movement = Movement.NewBuiltIn(
            "Front Squat",
            MovementCategoryId.New(),
            null
        );

        var act = () => movement.MakeVariantOf(movement.Id);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RemoveVariantParent_ShouldClearVariantParent_WhenParentExists()
    {
        var parentMovementId = MovementId.New();

        var movement = Movement.NewBuiltIn(
            "Front Squat",
            MovementCategoryId.New(),
            null
        );

        movement.MakeVariantOf(parentMovementId);

        movement.RemoveVariantParent();

        movement.VariantOfId.Should().BeNull();
        movement.VariantOf.Should().BeNull();
    }

    [Fact]
    public void RemoveVariantParent_ShouldDoNothing_WhenParentDoesNotExist()
    {
        var movement = Movement.NewBuiltIn(
            "Front Squat",
            MovementCategoryId.New(),
            null
        );

        movement.RemoveVariantParent();

        movement.VariantOfId.Should().BeNull();
        movement.VariantOf.Should().BeNull();
    }

    [Fact]
    public void AddVariant_ShouldAddVariantAndSetVariantParent()
    {
        var movement = Movement.NewBuiltIn(
            "Squat",
            MovementCategoryId.New(),
            null
        );

        var variant = Movement.NewBuiltIn(
            "Front Squat",
            MovementCategoryId.New(),
            null
        );

        movement.AddVariant(variant);

        movement.Variants.Should().ContainSingle();
        movement.Variants.First().Id.Should().Be(variant.Id);
        variant.VariantOfId.Should().Be(movement.Id);
    }

    [Fact]
    public void AddVariant_ShouldNotAddDuplicate_WhenVariantAlreadyExists()
    {
        var movement = Movement.NewBuiltIn(
            "Squat",
            MovementCategoryId.New(),
            null
        );

        var variant = Movement.NewBuiltIn(
            "Front Squat",
            MovementCategoryId.New(),
            null
        );

        movement.AddVariant(variant);
        movement.AddVariant(variant);

        movement.Variants.Should().ContainSingle();
    }

    [Fact]
    public void AddVariant_ShouldThrow_WhenMovementIsOwnVariant()
    {
        var movement = Movement.NewBuiltIn(
            "Squat",
            MovementCategoryId.New(),
            null
        );

        var act = () => movement.AddVariant(movement);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RemoveVariant_ShouldRemoveVariantAndClearVariantParent()
    {
        var movement = Movement.NewBuiltIn(
            "Squat",
            MovementCategoryId.New(),
            null
        );

        var variant = Movement.NewBuiltIn(
            "Front Squat",
            MovementCategoryId.New(),
            null
        );

        movement.AddVariant(variant);

        movement.RemoveVariant(variant.Id);

        movement.Variants.Should().BeEmpty();
        variant.VariantOfId.Should().BeNull();
        variant.VariantOf.Should().BeNull();
    }

    [Fact]
    public void RemoveVariant_ShouldDoNothing_WhenVariantDoesNotExist()
    {
        var movement = Movement.NewBuiltIn(
            "Squat",
            MovementCategoryId.New(),
            null
        );

        var variantId = MovementId.New();

        movement.RemoveVariant(variantId);

        movement.Variants.Should().BeEmpty();
    }

    [Fact]
    public void AddPattern_ShouldAddPattern_WhenPatternDoesNotExist()
    {
        var pattern = MovementPattern.Squat;

        var movement = Movement.NewBuiltIn(
            "Back Squat",
            MovementCategoryId.New(),
            null
        );

        movement.AddPattern(pattern);

        movement.Patterns.Should().ContainSingle();
        movement.Patterns.First().MovementId.Should().Be(movement.Id);
        movement.Patterns.First().Pattern.Should().Be(pattern);
    }

    [Fact]
    public void AddPattern_ShouldNotAddDuplicate_WhenPatternAlreadyExists()
    {
        var pattern = MovementPattern.Squat;

        var movement = Movement.NewBuiltIn(
            "Back Squat",
            MovementCategoryId.New(),
            null
        );

        movement.AddPattern(pattern);
        movement.AddPattern(pattern);

        movement.Patterns.Should().ContainSingle();
    }

    [Fact]
    public void AddPattern_ShouldThrow_WhenPatternIsComplex()
    {
        var pattern = MovementPattern.Complex;

        var movement = Movement.NewBuiltIn(
            "Clean And Jerk",
            MovementCategoryId.New(),
            null
        );

        var act = () => movement.AddPattern(pattern);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RemovePattern_ShouldRemovePattern_WhenPatternExists()
    {
        var pattern = MovementPattern.Squat;

        var movement = Movement.NewBuiltIn(
            "Back Squat",
            MovementCategoryId.New(),
            null
        );

        movement.AddPattern(pattern);

        movement.RemovePattern(pattern);

        movement.Patterns.Should().BeEmpty();
    }

    [Fact]
    public void RemovePattern_ShouldDoNothing_WhenPatternDoesNotExist()
    {
        var pattern = MovementPattern.Squat;

        var movement = Movement.NewBuiltIn(
            "Back Squat",
            MovementCategoryId.New(),
            null
        );

        movement.RemovePattern(pattern);

        movement.Patterns.Should().BeEmpty();
    }

    [Fact]
    public void SetPrimaryPattern_ShouldSetPrimaryPattern_AndAddPatternTag_WhenPatternIsNotComplex()
    {
        var pattern = MovementPattern.Squat;

        var movement = Movement.NewBuiltIn(
            "Back Squat",
            MovementCategoryId.New(),
            null
        );

        movement.SetPrimaryPattern(pattern);

        movement.PrimaryPattern.Should().Be(pattern);
        movement.Patterns.Should().ContainSingle();
        movement.Patterns.First().Pattern.Should().Be(pattern);
    }

    [Fact]
    public void SetPrimaryPattern_ShouldSetPrimaryPattern_WithoutAddingPatternTag_WhenPatternIsComplex()
    {
        var pattern = MovementPattern.Complex;

        var movement = Movement.NewBuiltIn(
            "Clean And Jerk",
            MovementCategoryId.New(),
            null
        );

        movement.SetPrimaryPattern(pattern);

        movement.PrimaryPattern.Should().Be(pattern);
        movement.Patterns.Should().BeEmpty();
    }

    [Fact]
    public void ClearPrimaryPattern_ShouldClearPrimaryPattern_WhenPrimaryPatternExists()
    {
        var pattern = MovementPattern.Squat;

        var movement = Movement.NewBuiltIn(
            "Back Squat",
            MovementCategoryId.New(),
            null
        );

        movement.SetPrimaryPattern(pattern);

        movement.ClearPrimaryPattern();

        movement.PrimaryPattern.Should().BeNull();
    }

    [Fact]
    public void ClearPrimaryPattern_ShouldDoNothing_WhenPrimaryPatternDoesNotExist()
    {
        var movement = Movement.NewBuiltIn(
            "Back Squat",
            MovementCategoryId.New(),
            null
        );

        movement.ClearPrimaryPattern();

        movement.PrimaryPattern.Should().BeNull();
    }

    [Fact]
    public void AutoResolvePrimaryPattern_ShouldSetPrimaryPatternToNull_WhenNoPatternExists()
    {
        var movement = Movement.NewBuiltIn(
            "Back Squat",
            MovementCategoryId.New(),
            null
        );

        movement.AutoResolvePrimaryPattern();

        movement.PrimaryPattern.Should().BeNull();
    }

    [Fact]
    public void AutoResolvePrimaryPattern_ShouldSetPrimaryPatternToOnlyPattern_WhenOnePatternExists()
    {
        var pattern = MovementPattern.Squat;

        var movement = Movement.NewBuiltIn(
            "Back Squat",
            MovementCategoryId.New(),
            null
        );

        movement.AddPattern(pattern);

        movement.AutoResolvePrimaryPattern();

        movement.PrimaryPattern.Should().Be(pattern);
    }

    [Fact]
    public void AutoResolvePrimaryPattern_ShouldSetPrimaryPatternToComplex_WhenMultiplePatternsExist()
    {
        var firstPattern = MovementPattern.Squat;
        var secondPattern = MovementPattern.Hinge;

        var movement = Movement.NewBuiltIn(
            "Thruster",
            MovementCategoryId.New(),
            null
        );

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

        var movement = Movement.NewBuiltIn(
            "Thruster",
            MovementCategoryId.New(),
            null
        );

        movement.AddPattern(firstPattern);
        movement.AddPattern(secondPattern);
        movement.SetPrimaryPattern(firstPattern);

        movement.RemovePattern(firstPattern);

        movement.PrimaryPattern.Should().Be(secondPattern);
    }

    [Fact]
    public void MarkDeleted_ShouldMarkMovementAsDeleted_WhenCalledThroughAuditedInterface()
    {
        var movement = Movement.NewBuiltIn(
            "Back Squat",
            MovementCategoryId.New(),
            null
        );

        ((IAudited)movement).MarkDeleted();

        movement.Audit.IsDeleted.Should().BeTrue();
    }
}