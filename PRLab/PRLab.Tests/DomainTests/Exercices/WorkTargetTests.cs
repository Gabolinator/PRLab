using FluentAssertions;
using PRLab.Domain.Value;
using PRLab.Domain.Value.Enum.Prescription;

namespace PRLab.Tests.DomainTests.Exercices;

public sealed class WorkTargetTests
{
    [Fact]
    public void New_ShouldCreateWorkTarget_WithValueAndTargetType()
    {
        var value = 10m;
        var targetType = WorkTargetType.Repetitions;

        var workTarget = WorkTarget.New(
            value,
            targetType
        );

        workTarget.Value.Should().Be(value);
        workTarget.TargetType.Should().Be(targetType);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void New_ShouldThrow_WhenValueIsNotGreaterThanZero(decimal value)
    {
        var targetType = WorkTargetType.Repetitions;

        var act = () => WorkTarget.New(
            value,
            targetType
        );

        act.Should().Throw<ArgumentException>();
    }
}