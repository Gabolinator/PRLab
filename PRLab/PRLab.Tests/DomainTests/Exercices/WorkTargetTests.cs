using FluentAssertions;
using PRLab.Domain;
using PRLab.Domain.Value;

namespace PRLab.Tests.DomainTests.Exercices;

public sealed class WorkTargetTests
{
    [Fact]
    public void New_ShouldCreateWorkTarget_WithValueAndTargetType()
    {
        var value = 10m;
        var targetType = DomainEnum.WorkTargetType.Repetitions;

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
        var targetType = DomainEnum.WorkTargetType.Repetitions;

        var act = () => WorkTarget.New(
            value,
            targetType
        );

        act.Should().Throw<ArgumentException>();
    }
}