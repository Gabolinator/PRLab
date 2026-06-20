namespace PRLab.Domain.Model.Value.Enum.Prescription.Work;

public enum SideExecutionMode
{
    NotApplicable = 1,

    AllRepsOneSideThenOther = 2,
    AlternatingEachRep = 3,
    AlternatingEachSet = 4,
    OneSideOnly = 5,
    UserChoice = 6
}