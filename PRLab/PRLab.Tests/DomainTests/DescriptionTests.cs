using FluentAssertions;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities;

namespace PRLab.Tests.DomainTests;

public sealed class DescriptionTests
{
    [Fact]
    public void New_ShouldCreateDescription_WithDefaultLanguageAndContent()
    {
        var content = "  Main description.  ";
        var language = LocalizationHelper.Language.EN;
        var languageCode = LocalizationHelper.ToLanguageCode(language);

        var description = Description.New(
            content,
            language
        );

        description.DefaultLanguageCode.Should().Be(languageCode);
        description.GetContent(language).Should().Be(content.Trim());
        description.GetResolvedLanguageCode(language).Should().Be(languageCode);
        description.Translations.Should().ContainSingle();
    }

    [Fact]
    public void New_ShouldNormalizeEmptyContentToNull()
    {
        var content = "   ";
        var language = LocalizationHelper.Language.EN;

        var description = Description.New(
            content,
            language
        );

        description.GetContent(language).Should().BeNull();
        description.Translations.Should().ContainSingle();
    }

    [Fact]
    public void GetContent_ShouldReturnRequestedLanguageContent_WhenTranslationExists()
    {
        var englishContent = "English content.";
        var frenchContent = "French content.";
        var description = Description.New(
            englishContent,
            LocalizationHelper.Language.EN
        );

        description.ChangeContent(
            frenchContent,
            LocalizationHelper.Language.FR
        );

        description.GetContent(LocalizationHelper.Language.FR).Should().Be(frenchContent);
    }

    [Fact]
    public void GetContent_ShouldReturnDefaultLanguageContent_WhenRequestedTranslationDoesNotExist()
    {
        var englishContent = "English content.";
        var description = Description.New(
            englishContent,
            LocalizationHelper.Language.EN
        );

        var content = description.GetContent(LocalizationHelper.Language.FR);

        content.Should().Be(englishContent);
    }

    [Fact]
    public void GetResolvedLanguageCode_ShouldReturnRequestedLanguageCode_WhenTranslationExists()
    {
        var englishContent = "English content.";
        var frenchContent = "French content.";
        var frenchLanguage = LocalizationHelper.Language.FR;
        var frenchLanguageCode = LocalizationHelper.ToLanguageCode(frenchLanguage);

        var description = Description.New(
            englishContent,
            LocalizationHelper.Language.EN
        );

        description.ChangeContent(
            frenchContent,
            frenchLanguage
        );

        description.GetResolvedLanguageCode(frenchLanguage).Should().Be(frenchLanguageCode);
    }

    [Fact]
    public void GetResolvedLanguageCode_ShouldReturnDefaultLanguageCode_WhenRequestedTranslationDoesNotExist()
    {
        var englishContent = "English content.";
        var defaultLanguage = LocalizationHelper.Language.EN;
        var defaultLanguageCode = LocalizationHelper.ToLanguageCode(defaultLanguage);

        var description = Description.New(
            englishContent,
            defaultLanguage
        );

        description.GetResolvedLanguageCode(LocalizationHelper.Language.FR).Should().Be(defaultLanguageCode);
    }

    [Fact]
    public void ChangeContent_ShouldUpdateExistingTranslation_WhenLanguageAlreadyExists()
    {
        var originalContent = "Original content.";
        var updatedContent = "  Updated content.  ";
        var language = LocalizationHelper.Language.EN;

        var description = Description.New(
            originalContent,
            language
        );

        description.ChangeContent(
            updatedContent,
            language
        );

        description.GetContent(language).Should().Be(updatedContent.Trim());
        description.Translations.Should().ContainSingle();
    }

    [Fact]
    public void ChangeContent_ShouldAddTranslation_WhenLanguageDoesNotExist()
    {
        var englishContent = "English content.";
        var frenchContent = "French content.";

        var description = Description.New(
            englishContent,
            LocalizationHelper.Language.EN
        );

        description.ChangeContent(
            frenchContent,
            LocalizationHelper.Language.FR
        );

        description.GetContent(LocalizationHelper.Language.FR).Should().Be(frenchContent);
        description.Translations.Should().HaveCount(2);
    }

    [Fact]
    public void RemoveContent_ShouldRemoveTranslation_WhenLanguageExists()
    {
        var englishContent = "English content.";
        var frenchContent = "French content.";

        var description = Description.New(
            englishContent,
            LocalizationHelper.Language.EN
        );

        description.ChangeContent(
            frenchContent,
            LocalizationHelper.Language.FR
        );

        description.RemoveContent(LocalizationHelper.Language.FR);

        description.GetContent(LocalizationHelper.Language.FR).Should().Be(englishContent);
        description.Translations.Should().ContainSingle();
    }

    [Fact]
    public void ChangeDefaultLanguage_ShouldChangeDefaultLanguage()
    {
        var englishContent = "English content.";
        var frenchContent = "French content.";
        var newDefaultLanguage = LocalizationHelper.Language.FR;
        var newDefaultLanguageCode = LocalizationHelper.ToLanguageCode(newDefaultLanguage);

        var description = Description.New(
            englishContent,
            LocalizationHelper.Language.EN
        );

        description.ChangeContent(
            frenchContent,
            newDefaultLanguage
        );

        description.ChangeDefaultLanguage(newDefaultLanguage);

        description.DefaultLanguageCode.Should().Be(newDefaultLanguageCode);
        description.GetContent(LocalizationHelper.Language.DE).Should().Be(frenchContent);
    }

    [Fact]
    public void Copy_ShouldCreateNewDescription_WithSameTranslations()
    {
        var englishContent = "English content.";
        var frenchContent = "French content.";
        var description = Description.New(
            englishContent,
            LocalizationHelper.Language.EN
        );

        description.ChangeContent(
            frenchContent,
            LocalizationHelper.Language.FR
        );

        var copy = description.Copy();

        copy.Id.Should().NotBe(description.Id);
        copy.DefaultLanguageCode.Should().Be(description.DefaultLanguageCode);
        copy.GetContent(LocalizationHelper.Language.EN).Should().Be(englishContent);
        copy.GetContent(LocalizationHelper.Language.FR).Should().Be(frenchContent);
        copy.Translations.Should().HaveCount(description.Translations.Count);
    }
}