using FluentAssertions;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Interface;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value.Enum.Movement;
using PRLab.Domain.Value.Enum.System;

namespace PRLab.Tests.DomainTests;

public sealed class MovementCategoryTests
{
    [Fact]
    public void NewBuiltIn_ShouldCreateMovementCategory_WithNormalizedNameAndNameKey()
    {
        var descriptionText = "Movements performed with bodyweight.";
        var baseMovementCategory = BaseMovementCategory.BodyWeight;

        var name = $" {baseMovementCategory.ToString()} ";

        var movementCategory = MovementCategory.NewBuiltIn(
            name,
            descriptionText,
            baseMovementCategory
        );

        movementCategory.Name.Should().Be(FormatingUtilities.NormalizeName(name));
        movementCategory.NameKey.Should().Be(FormatingUtilities.NormalizeNameKey(name));
        movementCategory.BaseMovementCategory.Should().Be(baseMovementCategory);
        movementCategory.Description.Should().NotBeNull();
        movementCategory.Description.GetContent().Should().Be(descriptionText);
        movementCategory.Audit.Should().NotBeNull();
        movementCategory.Audit.IsDeleted.Should().BeFalse();
        movementCategory.Ownership.Should().NotBeNull();
        movementCategory.Ownership.Origin.Should().Be(DataOrigin.BuiltIn);
        movementCategory.Ownership.OwnerUserId.Should().BeNull();
    }

    [Fact]
    public void NewBuiltIn_ShouldCreateMovementCategory_WithEmptyDescription_WhenDescriptionIsNull()
    {
        string? descriptionText = null;
        var baseMovementCategory = BaseMovementCategory.Resistance;
        var name = baseMovementCategory.ToString();

        var movementCategory = MovementCategory.NewBuiltIn(
            name,
            descriptionText,
            baseMovementCategory
        );

        movementCategory.Name.Should().Be(FormatingUtilities.NormalizeName(name));
        movementCategory.NameKey.Should().Be(FormatingUtilities.NormalizeNameKey(name));
        movementCategory.Description.Should().NotBeNull();
        movementCategory.Description.GetContent().Should().BeNull();
        movementCategory.Ownership.Origin.Should().Be(DataOrigin.BuiltIn);
        movementCategory.Ownership.OwnerUserId.Should().BeNull();
    }

    [Fact]
    public void NewBuiltIn_ShouldThrow_WhenNameIsEmpty()
    {
        var name = "   ";
        var descriptionText = "Invalid category.";
        var baseMovementCategory = BaseMovementCategory.BodyWeight;

        var act = () => MovementCategory.NewBuiltIn(
            name,
            descriptionText,
            baseMovementCategory
        );

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NewUserCreated_ShouldCreateMovementCategory_WithOwner()
    {
        var owner = User.New("Test User");
        var name = "  Custom Category  ";
        var descriptionText = "User-created movement category.";
        var baseMovementCategory = BaseMovementCategory.Hybrid;

        var movementCategory = MovementCategory.NewUserCreated(
            name,
            descriptionText,
            baseMovementCategory,
            owner
        );

        movementCategory.Name.Should().Be(FormatingUtilities.NormalizeName(name));
        movementCategory.NameKey.Should().Be(FormatingUtilities.NormalizeNameKey(name));
        movementCategory.BaseMovementCategory.Should().Be(baseMovementCategory);
        movementCategory.Description.GetContent().Should().Be(descriptionText);
        movementCategory.Audit.Should().NotBeNull();
        movementCategory.Audit.CreatedBy.Should().Be(owner.Id);
        movementCategory.Ownership.Should().NotBeNull();
        movementCategory.Ownership.Origin.Should().Be(DataOrigin.UserCreated);
        movementCategory.Ownership.OwnerUserId.Should().Be(owner.Id);
    }

    [Fact]
    public void ChangeDescription_ShouldChangeDescriptionContent()
    {
        var category = BaseMovementCategory.Cardio;
        var name = category.ToString();
        var initialDescriptionText = "Initial description.";
        var newDescriptionText = "Updated description.";
        var language = LocalizationHelper.Language.EN;

        var movementCategory = MovementCategory.NewBuiltIn(
            name,
            initialDescriptionText,
            category
        );

        movementCategory.ChangeDescription(
            newDescriptionText,
            language
        );

        movementCategory.Description.GetContent(language).Should().Be(newDescriptionText);
    }

    [Fact]
    public void ChangeDescription_ShouldMarkMovementCategoryAsUpdated()
    {
        var category = BaseMovementCategory.Cardio;
        var name = category.ToString();
        var initialDescriptionText = "Initial description.";
        var newDescriptionText = "Updated description.";
        var language = LocalizationHelper.Language.EN;

        var movementCategory = MovementCategory.NewBuiltIn(
            name,
            initialDescriptionText,
            category
        );

        var previousUpdatedAt = movementCategory.Audit.UpdatedAt;

        movementCategory.ChangeDescription(
            newDescriptionText,
            language
        );

        movementCategory.Audit.UpdatedAt.Should().NotBe(previousUpdatedAt);
    }

    [Fact]
    public void RemoveDescription_ShouldRemoveDescriptionContent()
    {
        var category = BaseMovementCategory.Mobility;
        var name = category.ToString();
        var descriptionText = "Mobility and stretching movements.";
        var language = LocalizationHelper.Language.EN;

        var movementCategory = MovementCategory.NewBuiltIn(
            name,
            descriptionText,
            category
        );

        movementCategory.RemoveDescription(language);

        movementCategory.Description.GetContent(language).Should().BeNull();
    }

    [Fact]
    public void RemoveDescription_ShouldMarkMovementCategoryAsUpdated()
    {
        var category = BaseMovementCategory.Mobility;
        var name = category.ToString();
        var descriptionText = "Mobility and stretching movements.";
        var language = LocalizationHelper.Language.EN;

        var movementCategory = MovementCategory.NewBuiltIn(
            name,
            descriptionText,
            category
        );

        var previousUpdatedAt = movementCategory.Audit.UpdatedAt;

        movementCategory.RemoveDescription(language);

        movementCategory.Audit.UpdatedAt.Should().NotBe(previousUpdatedAt);
    }

    [Fact]
    public void MarkDeleted_ShouldMarkMovementCategoryAsDeleted_WhenCalledThroughAuditedInterface()
    {
        var category = BaseMovementCategory.Hybrid;
        var name = category.ToString();
        var descriptionText = "Mixed movement category.";

        var movementCategory = MovementCategory.NewBuiltIn(
            name,
            descriptionText,
            category
        );

        ((IAudited)movementCategory).MarkDeleted();

        movementCategory.Audit.IsDeleted.Should().BeTrue();
    }

    [Theory]
    [InlineData("  Weightlifting  ", BaseMovementCategory.Resistance)]
    [InlineData("  Cable Machine  ", BaseMovementCategory.Resistance)]
    [InlineData("  Erg Machine  ", BaseMovementCategory.Cardio)]
    [InlineData("  Flexibility  ", BaseMovementCategory.Mobility)]
    public void NewBuiltIn_ShouldCreateCustomMovementCategory_WithExpectedBaseCategory(
        string categoryName,
        BaseMovementCategory baseMovementCategory)
    {
        var descriptionText = $"Custom category for {categoryName.Trim()} movements.";

        var movementCategory = MovementCategory.NewBuiltIn(
            categoryName,
            descriptionText,
            baseMovementCategory
        );

        movementCategory.Name.Should().Be(FormatingUtilities.NormalizeName(categoryName));
        movementCategory.NameKey.Should().Be(FormatingUtilities.NormalizeNameKey(categoryName));
        movementCategory.BaseMovementCategory.Should().Be(baseMovementCategory);
        movementCategory.Description.GetContent().Should().Be(descriptionText);
        movementCategory.Audit.Should().NotBeNull();
        movementCategory.Audit.IsDeleted.Should().BeFalse();
        movementCategory.Ownership.Origin.Should().Be(DataOrigin.BuiltIn);
    }
}