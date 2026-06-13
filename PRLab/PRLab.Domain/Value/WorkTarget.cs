using PRLab.Domain.Value.Enum.Prescription;

namespace PRLab.Domain.Value;

public sealed record WorkTarget
{
    public decimal Value { get; private set; }

    public WorkTargetType TargetType { get; private set; }

    private WorkTarget()
    {
        // EF Core
    }

    private WorkTarget(
        decimal value,
        WorkTargetType targetType)
    {
        Value = ValidateValue(value);
        TargetType = targetType;
    }

    public static WorkTarget New(
        decimal value,
        WorkTargetType repType)
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