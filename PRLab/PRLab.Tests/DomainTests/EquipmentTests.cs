using FluentAssertions;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Utilities;

namespace PRLab.Tests.DomainTests;

public sealed class EquipmentTests
{
    [Fact]
    public void NewBuiltIn_ShouldCreateEquipment_WithNormalizedName()
    {
        var descriptionText = "Used for pulling exercises.";
        var description = Description.New(descriptionText);
        var name = "  Pull-up bar  ";

        var equipment = Equipment.NewBuiltIn(
            name,
            description
        );

        equipment.Name.Should().Be(FormatingUtilities.NormalizeName(name));
        equipment.NameKey.Should().Be(FormatingUtilities.NormalizeNameKey(name));
        equipment.Description.Should().Be(description);
        equipment.Audit.Should().NotBeNull();
        equipment.Audit.IsDeleted.Should().BeFalse();
        equipment.Ownership.Should().NotBeNull();
        equipment.Ownership.Origin.Should().Be(DataOrigin.BuiltIn);
        equipment.Ownership.OwnerUserId.Should().BeNull();
    }

    [Fact]
    public void NewBuiltIn_ShouldThrow_WhenNameIsEmpty()
    {
        var description = Description.New("Test description.");

        var act = () => Equipment.NewBuiltIn(
            "   ",
            description
        );

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void NewUserCreated_ShouldCreateEquipment_WithOwner()
    {
        var owner = User.New("Test User");
        var description = Description.New("User-created equipment.");
        var name = "  Custom handle  ";

        var equipment = Equipment.NewUserCreated(
            name,
            description,
            owner
        );

        equipment.Name.Should().Be(FormatingUtilities.NormalizeName(name));
        equipment.NameKey.Should().Be(FormatingUtilities.NormalizeNameKey(name));
        equipment.Description.Should().Be(description);
        equipment.Audit.Should().NotBeNull();
        equipment.Audit.CreatedBy.Should().Be(owner.Id);
        equipment.Ownership.Should().NotBeNull();
        equipment.Ownership.Origin.Should().Be(DataOrigin.UserCreated);
        equipment.Ownership.OwnerUserId.Should().Be(owner.Id);
    }

    [Fact]
    public void Rename_ShouldChangeName_AndUpdateAudit()
    {
        var description = Description.New("Old description.");

        var equipment = Equipment.NewBuiltIn(
            "Barbell",
            description
        );

        var previousUpdatedAt = equipment.Audit.UpdatedAt;
        var name = "  Olympic barbell  ";

        equipment.Rename(name);

        equipment.Name.Should().Be(FormatingUtilities.NormalizeName(name));
        equipment.NameKey.Should().Be(FormatingUtilities.NormalizeNameKey(name));
        equipment.Audit.UpdatedAt.Should().NotBe(previousUpdatedAt);
    }

    [Fact]
    public void Rename_ShouldNotUpdateAudit_WhenNameIsSameAfterNormalization()
    {
        var description = Description.New("Old description.");

        var equipment = Equipment.NewBuiltIn(
            "Barbell",
            description
        );

        var previousUpdatedAt = equipment.Audit.UpdatedAt;

        equipment.Rename("  barbell  ");

        equipment.Name.Should().Be(FormatingUtilities.NormalizeName("Barbell"));
        equipment.NameKey.Should().Be(FormatingUtilities.NormalizeNameKey("Barbell"));
        equipment.Audit.UpdatedAt.Should().Be(previousUpdatedAt);
    }

    [Fact]
    public void ChangeDescription_ShouldReplaceDescriptionContent()
    {
        var description = Description.New("Old content.");

        var equipment = Equipment.NewBuiltIn(
            "Dumbbell",
            description
        );

        equipment.ChangeDescription("New content.", null);

        equipment.Description.GetContent().Should().Be("New content.");
    }
}