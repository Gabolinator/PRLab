namespace PRLab.Domain.Value;

public sealed record WorkTarget
{
    public decimal Value { get; private set; }

    public DomainEnum.RepType RepType { get; private set; }

    private WorkTarget()
    {
        // EF Core
    }

    private WorkTarget(
        decimal value,
        DomainEnum.RepType repType)
    {
        Value = ValidateValue(value);
        RepType = repType;
    }

    public static WorkTarget New(
        decimal value,
        DomainEnum.RepType repType)
    {
        return new WorkTarget(
            value,
            repType
        );
    }

    private static decimal ValidateValue(decimal value)
    {
        if (value <= 0)
        {
            throw new ArgumentException("Work target value must be greater than zero.");
        }

        return value;
    }
}