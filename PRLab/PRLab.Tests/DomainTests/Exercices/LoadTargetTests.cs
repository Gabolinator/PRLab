using FluentAssertions;
using PRLab.Domain;
using PRLab.Domain.Value;
using PRLab.Domain.Value.Enum.Prescription;

namespace PRLab.Tests.DomainTests.Exercices;

public sealed class LoadTargetTests
{
    [Fact]
    public void None_ShouldCreateEmptyLoadTarget()
    {
        var loadTarget = LoadTarget.None();

        loadTarget.Value.Should().BeNull();
        loadTarget.Type.Should().Be(LoadTargetType.None);
        loadTarget.Unit.Should().BeNull();
    }

    [Fact]
    public void BodyWeight_ShouldCreateBodyWeightLoadTarget()
    {
        var loadTarget = LoadTarget.BodyWeight();

        loadTarget.Value.Should().BeNull();
        loadTarget.Type.Should().Be(LoadTargetType.BodyWeight);
        loadTarget.Unit.Should().BeNull();
    }

    [Fact]
    public void ExternalLoad_ShouldCreateExternalLoadTarget()
    {
        var value = 100m;
        var unit = LoadUnit.Kilogram;

        var loadTarget = LoadTarget.ExternalLoad(
            value,
            unit
        );

        loadTarget.Value.Should().Be(value);
        loadTarget.Type.Should().Be(LoadTargetType.ExternalLoad);
        loadTarget.Unit.Should().Be(unit);
    }

    [Fact]
    public void AddedBodyWeightLoad_ShouldCreateAddedBodyWeightLoadTarget()
    {
        var value = 20m;
        var unit = LoadUnit.Kilogram;

        var loadTarget = LoadTarget.AddedBodyWeightLoad(
            value,
            unit
        );

        loadTarget.Value.Should().Be(value);
        loadTarget.Type.Should().Be(LoadTargetType.AddedBodyWeightLoad);
        loadTarget.Unit.Should().Be(unit);
    }

    [Fact]
    public void AssistedBodyWeight_ShouldCreateAssistedBodyWeightLoadTarget()
    {
        var value = 15m;
        var unit = LoadUnit.Kilogram;

        var loadTarget = LoadTarget.AssistedBodyWeight(
            value,
            unit
        );

        loadTarget.Value.Should().Be(value);
        loadTarget.Type.Should().Be(LoadTargetType.AssistedBodyWeight);
        loadTarget.Unit.Should().Be(unit);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void ExternalLoad_ShouldThrow_WhenValueIsNotGreaterThanZero(decimal value)
    {
        var unit = LoadUnit.Kilogram;

        var act = () => LoadTarget.ExternalLoad(
            value,
            unit
        );

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AddedBodyWeightLoad_ShouldThrow_WhenValueIsNotGreaterThanZero(decimal value)
    {
        var unit = LoadUnit.Kilogram;

        var act = () => LoadTarget.AddedBodyWeightLoad(
            value,
            unit
        );

        act.Should().Throw<ArgumentException>();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void AssistedBodyWeight_ShouldThrow_WhenValueIsNotGreaterThanZero(decimal value)
    {
        var unit = LoadUnit.Kilogram;

        var act = () => LoadTarget.AssistedBodyWeight(
            value,
            unit
        );

        act.Should().Throw<ArgumentException>();
    }
}