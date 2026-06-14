namespace PRLab.Domain.Model.Value.Enum.Prescription;

public enum WorkMode
{
    FixedWork,              // Complete prescribed work
    ForTime,                // Complete work as fast as possible
    MaxWorkInTime,          // AMRAP-style
    ForTimeWithCap,         // Complete work before cap
    EveryMinuteOnTheMinute, // EMOM
    Intervals               // Work/rest intervals
}