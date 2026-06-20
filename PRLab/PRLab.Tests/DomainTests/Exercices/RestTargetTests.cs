using FluentAssertions;
using PRLab.Domain.Model.Value;
using PRLab.Domain.Model.Value.Prescription;
using PRLab.Domain.Model.Value.Prescription.Rest;

namespace PRLab.Tests.DomainTests.Exercices;

public sealed class RestTargetTests
{
    [Fact]
    public void None_ShouldCreateEmptyRestTarget()
    {
        var restTarget = RestTarget.None();

        restTarget.Seconds.Should().BeNull();
        restTarget.IsEmpty().Should().BeTrue();
    }

    [Fact]
    public void SecondsDuration_ShouldCreateRestTarget_WithSeconds()
    {
        var seconds = 90;

        var restTarget = RestTarget.SecondsDuration(seconds);

        restTarget.Seconds.Should().Be(seconds);
        restTarget.IsEmpty().Should().BeFalse();
    }

    [Fact]
    public void SecondsDuration_ShouldAllowZeroSeconds()
    {
        var seconds = 0;

        var restTarget = RestTarget.SecondsDuration(seconds);

        restTarget.Seconds.Should().Be(seconds);
        restTarget.IsEmpty().Should().BeFalse();
    }

    [Fact]
    public void SecondsDuration_ShouldThrow_WhenSecondsIsNegative()
    {
        var seconds = -1;

        var act = () => RestTarget.SecondsDuration(seconds);

        act.Should().Throw<ArgumentException>();
    }
}