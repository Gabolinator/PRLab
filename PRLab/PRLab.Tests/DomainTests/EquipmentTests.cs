using FluentAssertions;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities;

namespace PRLab.Tests.DomainTests;

public sealed class EquipmentTests
{
    [Fact]
    public void New_ShouldCreateEquipment_WithNormalizedName()
    {
        var descriptionText = "Used for pulling exercises.";
        var description = Description.New(descriptionText);
        var name = "  Pull-up bar  ";
        var equipment = Equipment.New(
            name,
            description
        );

        equipment.Name.Should().Be(FormatingUtilities.NormalizeName(name));
        equipment.Description.Should().Be(description);
        equipment.Audit.Should().NotBeNull();
        equipment.Audit.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void New_ShouldThrow_WhenNameIsEmpty()
    {
        var description = Description.New("Test description.");

        var act = () => Equipment.New(
            "   ",
            description
        );

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Rename_ShouldChangeName_AndUpdateAudit()
    {
        var description = Description.New("Old description.");

        var equipment = Equipment.New(
            "Barbell",
            description
        );

        var previousUpdatedAt = equipment.Audit.UpdatedAt;
        var name = "  Olympic barbell  ";
        equipment.Rename(name);

        equipment.Name.Should().Be(FormatingUtilities.NormalizeName(name));
        equipment.Audit.UpdatedAt.Should().NotBe(previousUpdatedAt);
    }

    [Fact]
    public void ChangeDescription_ShouldReplaceDescriptionContent()
    {
        var description = Description.New("Old content.");

        var equipment = Equipment.New(
            "Dumbbell",
            description
        );

        equipment.ChangeDescription("New content.", null);

        equipment.Description.GetContent().Should().Be("New content.");
    }
}