using FluentAssertions;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value.Enum.Anatomy;
using PRLab.Domain.Value.Identifier;
using PRLab.Domain.Value.Update;

namespace PRLab.Tests.DomainTests;

public sealed class MuscleTests
{
    [Fact]
    public void New_ShouldCreateMuscle_WithNormalizedNameAndNameKey()
    {
        var name = "  Pectoralis Major  ";
        var latinName = "  Pectoralis major  ";
        var bodySection = BodySection.UpperBody;
        var descriptionText = "Chest muscle.";
        var description = Description.New(descriptionText);

        var muscle = Muscle.New(
            name,
            latinName,
            bodySection,
            description
        );

        muscle.Name.Should().Be(FormatingUtilities.NormalizeName(name));
        muscle.NameKey.Should().Be(FormatingUtilities.NormalizeNameKey(name));
        muscle.LatinName.Should().Be(latinName.Trim());
        muscle.BodySection.Should().Be(bodySection);
        muscle.Description.Should().Be(description);
        muscle.Audit.Should().NotBeNull();
        muscle.Audit.IsDeleted.Should().BeFalse();
        muscle.Antagonists.Should().BeEmpty();
    }

    [Fact]
    public void New_ShouldNormalizeEmptyLatinNameToNull()
    {
        var name = "Biceps";
        var latinName = "   ";
        var bodySection = BodySection.UpperBody;
        var description = Description.New(null);

        var muscle = Muscle.New(
            name,
            latinName,
            bodySection,
            description
        );

        muscle.LatinName.Should().BeNull();
    }

    [Fact]
    public void New_ShouldThrow_WhenDescriptionIsNull()
    {
        var name = "Biceps";
        var latinName = "Biceps brachii";
        var bodySection = BodySection.UpperBody;
        Description description = null!;

        var act = () => Muscle.New(
            name,
            latinName,
            bodySection,
            description
        );

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Rename_ShouldUpdateNameAndNameKey()
    {
        var initialName = "Biceps";
        var newName = "  Biceps Brachii  ";
        var muscle = Muscle.New(
            initialName,
            null,
            BodySection.UpperBody,
            Description.New(null)
        );

        muscle.Rename(newName);

        muscle.Name.Should().Be(FormatingUtilities.NormalizeName(newName));
        muscle.NameKey.Should().Be(FormatingUtilities.NormalizeNameKey(newName));
    }

    [Fact]
    public void Rename_ShouldMarkMuscleAsUpdated()
    {
        var initialName = "Biceps";
        var newName = "Biceps Brachii";
        var muscle = Muscle.New(
            initialName,
            null,
            BodySection.UpperBody,
            Description.New(null)
        );

        var previousUpdatedAt = muscle.Audit.UpdatedAt;

        muscle.Rename(newName);

        muscle.Audit.UpdatedAt.Should().NotBe(previousUpdatedAt);
    }

    [Fact]
    public void ChangeLatinName_ShouldTrimLatinName()
    {
        var name = "Biceps";
        var latinName = "  Biceps brachii  ";
        var muscle = Muscle.New(
            name,
            null,
            BodySection.UpperBody,
            Description.New(null)
        );

        muscle.ChangeLatinName(latinName);

        muscle.LatinName.Should().Be(latinName.Trim());
    }

    [Fact]
    public void ChangeLatinName_ShouldSetLatinNameToNull_WhenValueIsWhiteSpace()
    {
        var name = "Biceps";
        var initialLatinName = "Biceps brachii";
        var newLatinName = "   ";
        var muscle = Muscle.New(
            name,
            initialLatinName,
            BodySection.UpperBody,
            Description.New(null)
        );

        muscle.ChangeLatinName(newLatinName);

        muscle.LatinName.Should().BeNull();
    }

    [Fact]
    public void ChangeBodySection_ShouldUpdateBodySection()
    {
        var name = "Rectus abdominis";
        var newBodySection = BodySection.MidSection;
        var muscle = Muscle.New(
            name,
            null,
            BodySection.UpperBody,
            Description.New(null)
        );

        muscle.ChangeBodySection(newBodySection);

        muscle.BodySection.Should().Be(newBodySection);
    }

    [Fact]
    public void AddAntagonist_ShouldAddAntagonist_WhenNotAlreadyAdded()
    {
        var name = "Biceps";
        var antagonistMuscleId = MuscleId.New();
        var muscle = Muscle.New(
            name,
            null,
            BodySection.UpperBody,
            Description.New(null)
        );

        muscle.AddAntagonist(antagonistMuscleId);

        muscle.Antagonists.Should().ContainSingle();
        muscle.Antagonists.First().MuscleId.Should().Be(muscle.Id);
        muscle.Antagonists.First().AntagonistMuscleId.Should().Be(antagonistMuscleId);
    }

    [Fact]
    public void AddAntagonist_ShouldNotAddDuplicate_WhenAntagonistAlreadyExists()
    {
        var name = "Biceps";
        var antagonistMuscleId = MuscleId.New();
        var muscle = Muscle.New(
            name,
            null,
            BodySection.UpperBody,
            Description.New(null)
        );

        muscle.AddAntagonist(antagonistMuscleId);
        muscle.AddAntagonist(antagonistMuscleId);

        muscle.Antagonists.Should().ContainSingle();
    }

    [Fact]
    public void AddAntagonist_ShouldThrow_WhenMuscleIsOwnAntagonist()
    {
        var name = "Biceps";
        var muscle = Muscle.New(
            name,
            null,
            BodySection.UpperBody,
            Description.New(null)
        );

        var act = () => muscle.AddAntagonist(muscle.Id);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void RemoveAntagonist_ShouldRemoveAntagonist_WhenRelationExists()
    {
        var name = "Biceps";
        var antagonistMuscleId = MuscleId.New();
        var muscle = Muscle.New(
            name,
            null,
            BodySection.UpperBody,
            Description.New(null)
        );

        muscle.AddAntagonist(antagonistMuscleId);

        muscle.RemoveAntagonist(antagonistMuscleId);

        muscle.Antagonists.Should().BeEmpty();
    }

    [Fact]
    public void RemoveAntagonist_ShouldDoNothing_WhenRelationDoesNotExist()
    {
        var name = "Biceps";
        var antagonistMuscleId = MuscleId.New();
        var muscle = Muscle.New(
            name,
            null,
            BodySection.UpperBody,
            Description.New(null)
        );

        muscle.RemoveAntagonist(antagonistMuscleId);

        muscle.Antagonists.Should().BeEmpty();
    }

    [Fact]
    public void ChangeDescription_ShouldChangeDescriptionContent()
    {
        var name = "Biceps";
        var initialDescriptionText = "Initial description.";
        var newDescriptionText = "Updated description.";
        var language = LocalizationHelper.Language.EN;

        var muscle = Muscle.New(
            name,
            null,
            BodySection.UpperBody,
            Description.New(initialDescriptionText, language)
        );

        muscle.ChangeDescription(
            newDescriptionText,
            language
        );

        muscle.Description.GetContent(language).Should().Be(newDescriptionText);
    }

    [Fact]
    public void RemoveDescription_ShouldRemoveDescriptionContent()
    {
        var name = "Biceps";
        var descriptionText = "Initial description.";
        var language = LocalizationHelper.Language.EN;

        var muscle = Muscle.New(
            name,
            null,
            BodySection.UpperBody,
            Description.New(descriptionText, language)
        );

        muscle.RemoveDescription(language);

        muscle.Description.GetContent(language).Should().BeNull();
    }

    [Fact]
    public void Update_ShouldUpdateProvidedValues()
    {
        var initialName = "Biceps";
        var newName = "  Biceps Brachii  ";
        var newLatinName = "  Biceps brachii  ";
        var newBodySection = BodySection.UpperBody;
        var newDescriptionText = "Updated description.";

        var muscle = Muscle.New(
            initialName,
            null,
            BodySection.LowerBody,
            Description.New(null)
        );

        var update = new MuscleUpdate
        {
            Name = newName,
            LatinName = newLatinName,
            BodySection = newBodySection,
            LatinNameWasProvided = true,
            DescriptionUpdate = new DescriptionUpdate
            {
                Content = newDescriptionText,
                Language = LocalizationHelper.Language.EN
            }
        };

        muscle.Update(update);

        muscle.Name.Should().Be(FormatingUtilities.NormalizeName(newName));
        muscle.NameKey.Should().Be(FormatingUtilities.NormalizeNameKey(newName));
        muscle.LatinName.Should().Be(newLatinName.Trim());
        muscle.BodySection.Should().Be(newBodySection);
        muscle.Description.GetContent(LocalizationHelper.Language.EN).Should().Be(newDescriptionText);
    }
}