using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.Movement;
using PRLab.Domain.Model.Value.Enum.Prescription.Load;

namespace PRLab.Domain.Model.Value.Prescription.Load;

public sealed record LoadTarget
{
    public decimal? Value { get; private set; }

    public LoadTargetType Type { get; private set; }

    public LoadUnit? Unit { get; private set; }

    public LoadReference? LoadReference { get; private set; }

    public int? ReferenceRepMax { get; private set; }

    private LoadTarget()
    {
        // EF Core
    }

    private LoadTarget(
        decimal? value,
        LoadTargetType type,
        LoadUnit? unit,
        LoadReference? loadReference = null,
        int? referenceRepMax = null)
    {
        ValidateOrThrow(
            value,
            type,
            unit,
            loadReference,
            referenceRepMax);

        Value = value;
        Type = type;
        Unit = unit;
        LoadReference = loadReference;
        ReferenceRepMax = referenceRepMax;
    }

    public static LoadTarget BodyWeight()
    {
        return new LoadTarget(
            null,
            LoadTargetType.BodyWeight,
            null);
    }

    public static LoadTarget RepMax(
        int reps,
        LoadReference? loadReference = null)
    {
        return new LoadTarget(
            reps,
            LoadTargetType.RepMax,
            LoadUnit.RM,
            loadReference);
    }

    public static LoadTarget PercentageRepMax(
        decimal percent,
        LoadReference? loadReference = null,
        int referenceRepMax = 1)
    {
        return new LoadTarget(
            percent,
            LoadTargetType.RepMax,
            LoadUnit.Percent,
            loadReference,
            referenceRepMax);
    }

    public static LoadTarget ExternalLoad(
        decimal value,
        LoadUnit unit)
    {
        return new LoadTarget(
            value,
            LoadTargetType.ExternalLoad,
            unit);
    }

    public static LoadTarget AddedBodyWeightLoad(
        decimal value,
        LoadUnit unit)
    {
        return new LoadTarget(
            value,
            LoadTargetType.AddedBodyWeightLoad,
            unit);
    }

    public static LoadTarget AssistedBodyWeight(
        decimal value,
        LoadUnit unit)
    {
        return new LoadTarget(
            value,
            LoadTargetType.AssistedBodyWeight,
            unit);
    }

    public static LoadTarget None()
    {
        return new LoadTarget(
            null,
            LoadTargetType.None,
            null);
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

    private static void ValidateOrThrow(
        decimal? value,
        LoadTargetType type,
        LoadUnit? unit,
        LoadReference? loadReference,
        int? referenceRepMax)
    {
        ValidateValue(value);
        ValidateReferenceRepMax(referenceRepMax);

        switch (type)
        {
            case LoadTargetType.None:
            case LoadTargetType.BodyWeight:
                ValidateNoValueUnitOrReference(
                    type,
                    value,
                    unit,
                    loadReference,
                    referenceRepMax);
                break;

            case LoadTargetType.RepMax:
                ValidateRepMax(
                    value,
                    unit,
                    referenceRepMax);
                break;

            case LoadTargetType.ExternalLoad:
            case LoadTargetType.AddedBodyWeightLoad:
            case LoadTargetType.AssistedBodyWeight:
                ValidateConcreteLoad(
                    type,
                    value,
                    unit,
                    loadReference,
                    referenceRepMax);
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(type),
                    type,
                    "Unsupported load target type.");
        }
    }

    private static void ValidateNoValueUnitOrReference(
        LoadTargetType type,
        decimal? value,
        LoadUnit? unit,
        LoadReference? loadReference,
        int? referenceRepMax)
    {
        if (value.HasValue)
        {
            throw new ArgumentException($"{type} load target should not define a value.", nameof(value));
        }

        if (unit.HasValue)
        {
            throw new ArgumentException($"{type} load target should not define a unit.", nameof(unit));
        }

        if (loadReference is not null)
        {
            throw new ArgumentException($"{type} load target should not define a load reference.", nameof(loadReference));
        }

        if (referenceRepMax.HasValue)
        {
            throw new ArgumentException($"{type} load target should not define a reference rep max.", nameof(referenceRepMax));
        }
    }

    private static void ValidateRepMax(
        decimal? value,
        LoadUnit? unit,
        int? referenceRepMax)
    {
        if (!value.HasValue)
        {
            throw new ArgumentException("Rep max load target requires a value.", nameof(value));
        }

        if (unit is not LoadUnit.RM && unit is not LoadUnit.Percent)
        {
            throw new ArgumentException("Rep max load target must use RM or Percent unit.", nameof(unit));
        }

        if (unit == LoadUnit.RM && value.Value % 1 != 0)
        {
            throw new ArgumentException("RM target value must be a whole number.", nameof(value));
        }

        if (unit == LoadUnit.Percent)
        {
            if (value.Value > 100)
            {
                throw new ArgumentException("Percentage rep max cannot be greater than 100.", nameof(value));
            }

            if (!referenceRepMax.HasValue)
            {
                throw new ArgumentException("Percentage rep max requires a reference rep max.", nameof(referenceRepMax));
            }
        }

        if (unit == LoadUnit.RM && referenceRepMax.HasValue)
        {
            throw new ArgumentException("Direct RM target should not define a reference rep max.", nameof(referenceRepMax));
        }
    }

    private static void ValidateConcreteLoad(
        LoadTargetType type,
        decimal? value,
        LoadUnit? unit,
        LoadReference? loadReference,
        int? referenceRepMax)
    {
        if (!value.HasValue)
        {
            throw new ArgumentException($"{type} load target requires a value.", nameof(value));
        }

        if (!unit.HasValue)
        {
            throw new ArgumentException($"{type} load target requires a unit.", nameof(unit));
        }

        if (unit is LoadUnit.RM)
        {
            throw new ArgumentException($"{type} load target cannot use RM unit.", nameof(unit));
        }

        if (loadReference is not null)
        {
            throw new ArgumentException($"{type} load target should not define a load reference.", nameof(loadReference));
        }

        if (referenceRepMax.HasValue)
        {
            throw new ArgumentException($"{type} load target should not define a reference rep max.", nameof(referenceRepMax));
        }
    }

    private static void ValidateValue(decimal? value)
    {
        if (value.HasValue && value.Value <= 0)
        {
            throw new ArgumentException("Load target value must be greater than zero.", nameof(value));
        }
    }

    private static void ValidateReferenceRepMax(int? referenceRepMax)
    {
        if (referenceRepMax.HasValue && referenceRepMax.Value < 1)
        {
            throw new ArgumentException("Reference rep max must be greater than zero.", nameof(referenceRepMax));
        }
    }
}