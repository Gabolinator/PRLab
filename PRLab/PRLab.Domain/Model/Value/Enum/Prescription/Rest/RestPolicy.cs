namespace PRLab.Domain.Model.Value.Enum.Prescription.Rest;

public enum RestPolicy
{
    None,
    Fixed,
    NoMoreThan,
    AtLeast,
    Range,
    AsNeeded,
    UntilRecovered
}