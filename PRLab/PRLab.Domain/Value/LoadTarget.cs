namespace PRLab.Domain.Value;

public sealed record LoadTarget
{
    public decimal? Value { get; private set; }

    public DomainEnum.LoadTargetType Type { get; private set; }

    public DomainEnum.LoadUnit? Unit { get; private set; }

    private LoadTarget()
    {
        // EF Core
    }

    private LoadTarget(
        decimal? value,
        DomainEnum.LoadTargetType type,
        DomainEnum.LoadUnit? unit)
    {
        Value = ValidateValue(value);
        Type = type;
        Unit = unit;
    }

    public static LoadTarget BodyWeight()
    {
        return new LoadTarget(
            null,
            DomainEnum.LoadTargetType.BodyWeight,
            null
        );
    }

    public static LoadTarget ExternalLoad(
        decimal value,
        DomainEnum.LoadUnit unit)
    {
        return new LoadTarget(
            value,
            DomainEnum.LoadTargetType.ExternalLoad,
            unit
        );
    }

    public static LoadTarget AddedBodyWeightLoad(
        decimal value,
        DomainEnum.LoadUnit unit)
    {
        return new LoadTarget(
            value,
            DomainEnum.LoadTargetType.AddedBodyWeightLoad,
            unit
        );
    }

    public static LoadTarget AssistedBodyWeight(
        decimal value,
        DomainEnum.LoadUnit unit)
    {
        return new LoadTarget(
            value,
            DomainEnum.LoadTargetType.AssistedBodyWeight,
            unit
        );
    }

    public static LoadTarget None()
    {
        return new LoadTarget(
            null,
            DomainEnum.LoadTargetType.None,
            null
        );
    }

    private static decimal? ValidateValue(decimal? value)
    {
        if (value is null)
        {
            return null;
        }

        if (value <= 0)
        {
            throw new ArgumentException("Load target value must be greater than zero.");
        }

        return value;
    }
}