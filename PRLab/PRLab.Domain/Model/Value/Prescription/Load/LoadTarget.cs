using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.Movement;
using PRLab.Domain.Model.Value.Enum.Prescription;
using PRLab.Domain.Model.Value.Enum.Prescription.Load;

namespace PRLab.Domain.Model.Value.Prescription.Load;

public sealed record LoadTarget
{
    public decimal? Value { get; private set; }

    public LoadTargetType Type { get; private set; }

    public LoadUnit? Unit { get; private set; }
    
    //todo hook
    public LoadReference? LoadReference { get; private set; }

    private LoadTarget()
    {
        // EF Core
    }

    private LoadTarget(
        decimal? value,
        LoadTargetType type,
        LoadUnit? unit)
    {
        Value = ValidateValue(value);
        Type = type;
        Unit = unit;
    }

    public static LoadTarget BodyWeight()
    {
        return new LoadTarget(
            null,
            LoadTargetType.BodyWeight,
            null
        );
    }

    public static LoadTarget RepMax(int value)
    {
        return new LoadTarget(
            value,
            LoadTargetType.RepMax,
            LoadUnit.RM);
    }
    
    public static LoadTarget PercentageRepMax(int value)
    {
        return new LoadTarget(
            value,
            LoadTargetType.RepMax,
            LoadUnit.Percent);
    }
    
    public static LoadTarget ExternalLoad(
        decimal value,
        LoadUnit unit)
    {
        return new LoadTarget(
            value,
            LoadTargetType.ExternalLoad,
            unit
        );
    }

    public static LoadTarget AddedBodyWeightLoad(
        decimal value,
        LoadUnit unit)
    {
        return new LoadTarget(
            value,
            LoadTargetType.AddedBodyWeightLoad,
            unit
        );
    }

    public static LoadTarget AssistedBodyWeight(
        decimal value,
        LoadUnit unit)
    {
        return new LoadTarget(
            value,
            LoadTargetType.AssistedBodyWeight,
            unit
        );
    }

    public static LoadTarget None()
    {
        return new LoadTarget(
            null,
            LoadTargetType.None,
            null
        );
    }
    
    public static LoadTarget FromMovement(Movement movement)
    {
        ArgumentNullException.ThrowIfNull(movement);

        return movement.MovementCategory.BaseMovementCategory switch
        {
            BaseMovementCategory.BodyWeight => BodyWeight(),

            _ => None()
        };
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