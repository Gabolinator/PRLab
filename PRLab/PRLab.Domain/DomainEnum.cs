namespace PRLab.Domain;

public static class DomainEnum
{

  public enum MovementPattern
  {
    Squat,
    Hinge,
    Push,
    Pull,
    Lunge,
    Carry,
    Rotation,
    AntiRotation,
    Gait,
    Jump,
    Crawl,
    Balance,
    Mobility,
    Isolation,
    Complex,
  }

  public enum BaseMovementCategory
  {
    BodyWeight,
    Resistance,
    Cardio,
    Mobility,
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
  
  public enum RepPhaseExecutionIntent
  {
    Standard = 1,
    Controlled = 2,
    Explosive = 3,
    Paused = 4,
    Strict = 5,
    IsometricHold = 6,
    Rebound = 7,
    Stabilized = 8,
  }

  public enum WorkTargetType
  {
    Repetitions = 1,
    DurationSeconds = 2,
    DistanceMeters = 3,
    Calories = 4,
    TimeUnderTensionSeconds = 5,
  }
  
  public enum WorkMode
  {
    FixedWork = 1,
    ForTime = 2,
    MaxWorkInTime = 3,
    ForTimeWithCap = 4,
    EveryMinuteOnTheMinute = 5,
    Intervals = 6,
  }
  
  public enum LoadTargetType
  {
    None = 1,
    BodyWeight = 2,
    ExternalLoad = 3,
    AddedBodyWeightLoad = 4,
    AssistedBodyWeight = 5,
    ResistanceBand = 6,
    MachineSetting = 7,
    PercentageOfOneRepMax = 8,
    RateOfPerceivedExertion = 9,
    RepsInReserve = 10,
  }

  public enum LoadUnit
  {
    Kilogram = 1,
    Pound = 2,
    Percent = 3,
    Level = 4,
  }
  
  public enum UserRole
  {
    User = 1,
    Coach = 2,
    Admin = 3,
  }
  
  public enum EntityType
  {
    User, 
    Description,
    Equipment,
    MovementCategory,
    Muscle,
    MuscleAntagonist,
    Movement,
    WorkloadProfile,
    Exercise,
    Workout,
    Program,
  }
  
}