namespace PRLab.Domain.Model.Value.Enum.Prescription;

public enum WorkPartitionStrategy
{
    Repeated,    // repeat the segment/step N times
    SplitAnyhow, // complete total work in any partition
    VariableRepeats, //ex for ladder , etc
    Unbroken     // complete without breaking
}