using FluentAssertions;
using PRLab.Domain;
using PRLab.Domain.Value;

namespace PRLab.Tests.DomainTests.Exercices;

public sealed class RepExecutionDetailsTests
{
    [Fact]
    public void Empty_ShouldCreateEmptyExecutionDetails()
    {
        var executionDetails = RepExecutionDetails.Empty();

        executionDetails.EccentricSeconds.Should().BeNull();
        executionDetails.BottomPauseSeconds.Should().BeNull();
        executionDetails.ConcentricSeconds.Should().BeNull();
        executionDetails.TopPauseSeconds.Should().BeNull();

        executionDetails.EccentricIntent.Should().BeNull();
        executionDetails.BottomIntent.Should().BeNull();
        executionDetails.ConcentricIntent.Should().BeNull();
        executionDetails.TopIntent.Should().BeNull();

        executionDetails.Intent.Should().BeNull();
        executionDetails.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void New_ShouldCreateExecutionDetails_WithProvidedValues()
    {
        var eccentricSeconds = 3;
        var bottomPauseSeconds = 1;
        var concentricSeconds = 2;
        var topPauseSeconds = 1;

        var eccentricIntent = DomainEnum.RepPhaseExecutionIntent.Controlled;
        var bottomIntent = DomainEnum.RepPhaseExecutionIntent.Paused;
        var concentricIntent = DomainEnum.RepPhaseExecutionIntent.Explosive;
        var topIntent = DomainEnum.RepPhaseExecutionIntent.Strict;

        var intent = "  Keep perfect form.  ";

        var executionDetails = RepExecutionDetails.New(
            eccentricSeconds,
            bottomPauseSeconds,
            concentricSeconds,
            topPauseSeconds,
            eccentricIntent,
            bottomIntent,
            concentricIntent,
            topIntent,
            intent
        );

        executionDetails.EccentricSeconds.Should().Be(eccentricSeconds);
        executionDetails.BottomPauseSeconds.Should().Be(bottomPauseSeconds);
        executionDetails.ConcentricSeconds.Should().Be(concentricSeconds);
        executionDetails.TopPauseSeconds.Should().Be(topPauseSeconds);

        executionDetails.EccentricIntent.Should().Be(eccentricIntent);
        executionDetails.BottomIntent.Should().Be(bottomIntent);
        executionDetails.ConcentricIntent.Should().Be(concentricIntent);
        executionDetails.TopIntent.Should().Be(topIntent);

        executionDetails.Intent.Should().Be(intent.Trim());
        executionDetails.IsEmpty().Should().BeFalse();
    }

    [Fact]
    public void New_ShouldNormalizeEmptyIntentToNull()
    {
        var intent = "   ";

        var executionDetails = RepExecutionDetails.New(
            intent: intent
        );

        executionDetails.Intent.Should().BeNull();
        executionDetails.IsEmpty().Should().BeTrue();
    }

    [Theory]
    [InlineData(-1, null, null, null)]
    [InlineData(null, -1, null, null)]
    [InlineData(null, null, -1, null)]
    [InlineData(null, null, null, -1)]
    public void New_ShouldThrow_WhenAnySecondsValueIsNegative(
        int? eccentricSeconds,
        int? bottomPauseSeconds,
        int? concentricSeconds,
        int? topPauseSeconds)
    {
        var act = () => RepExecutionDetails.New(
            eccentricSeconds,
            bottomPauseSeconds,
            concentricSeconds,
            topPauseSeconds
        );

        act.Should().Throw<ArgumentException>();
    }
}