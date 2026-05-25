namespace PRLab;

public static class DomainEnum
{
  public enum BaseMovementCategory
  {
    BodyWeight,
    Weightlifting,
    Cardio,
    Flexibility,
    Hybrid,
  }
  
  public enum BodySection
  {
    UpperBody,
    MidSection,
    LowerBody,
  }

  public enum MuscleRole
  {
    Primary = 1,
    Secondary = 2,
    Stabilizer = 3
  }
  
}