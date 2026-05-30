namespace PRLab.Domain.Value;

public sealed record WorkTarget
{
    public decimal Value { get; private set; }

    public DomainEnum.WorkTargetType TargetType { get; private set; }

    private WorkTarget()
    {
        // EF Core
    }

    private WorkTarget(
        decimal value,
        DomainEnum.WorkTargetType targetType)
    {
        Value = ValidateValue(value);
        TargetType = targetType;
    }

    public static WorkTarget New(
        decimal value,
        DomainEnum.WorkTargetType repType)
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