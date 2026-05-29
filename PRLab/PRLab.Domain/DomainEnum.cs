namespace PRLab.Domain;

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

  public enum RepType
  {
    TimeUnderTension = 1,
    Distance = 2,
    Repetitions = 3,
    Calories = 4,
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
  public enum UserRole
  {
    User = 1,
    Coach = 2,
    Admin = 3,
  }
  
  public enum EntityType
  {
    User, 
    Descriptor,
    Equipment,
    EquipmentList,
    MovementCategory,
    Muscle,
    MuscleGroup,
    Movement,
    WorkloadProfile,
    Exercise,
    ExerciseBlock,
    WorkoutBlock,
    Workout,
    Program,
    unidentified,
  }
  
}