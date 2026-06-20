using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Model.Value.WorkoutValue;
using PRLab.Domain.Utilities;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.DB.ModelBuilders;
using PRLab.Infrastructure.DB.ModelBuilders.Entity;
using PRLab.Infrastructure.DB.Seeding;

namespace PRLab.Infrastructure.DB.Context;

// db context for postgres
public class PRLabPgDBContext(DbContextOptions<PRLabPgDBContext> options) : DbContext(options)
{
    private IAppLogger? logger;
    private IClock? clock;

    #region DB SETS

    public DbSet<SeedHistory> SeedHistory => Set<SeedHistory>();

    // descriptions
    public DbSet<Description> Description => Set<Description>();
    public DbSet<DescriptionTranslation> DescriptionTranslations => Set<DescriptionTranslation>();

    // equipment
    public DbSet<Equipment> Equipments => Set<Equipment>();

    // muscles
    public DbSet<Muscle> Muscles => Set<Muscle>();
    public DbSet<MuscleAntagonist> MuscleAntagonists => Set<MuscleAntagonist>();

    // categories
    public DbSet<MovementCategory> MovementCategories => Set<MovementCategory>();

    // movement
    public DbSet<Movement> Movements => Set<Movement>();

    // exercises
    public DbSet<Exercise> Exercises => Set<Exercise>();

    // workouts
    public DbSet<Workout> Workouts => Set<Workout>();
    public DbSet<WorkoutBlock> WorkoutBlocks => Set<WorkoutBlock>();
    public DbSet<WorkoutBlockAssignment> WorkoutBlockAssignments => Set<WorkoutBlockAssignment>();
    public DbSet<WorkoutBlockSegment> WorkoutBlockSegments => Set<WorkoutBlockSegment>();
    public DbSet<WorkoutBlockSegmentStep> WorkoutBlockSegmentSteps => Set<WorkoutBlockSegmentStep>();

    // user
    public DbSet<User> Users => Set<User>();

    #endregion

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        logger?.Log("PRLabPgDBContext", "Model Creating");

        modelBuilder.HasDefaultSchema("public");

        modelBuilder.AddSeedHistoryTableModel();

        modelBuilder.AddUserTableModel();

        modelBuilder.AddEquipmentTableModel();

        modelBuilder.AddDescriptionTableModels();

        modelBuilder.AddMuscleTableModels();

        modelBuilder.AddMovementCategoryTableModel();

        modelBuilder.AddMovementTableModels();

        modelBuilder.AddExerciseTableModels();

        modelBuilder.AddWorkoutTableModels();

        AddIndexes(modelBuilder);
    }

    private void AddIndexes(ModelBuilder modelBuilder)
    {
        modelBuilder.AddDescriptionIndexes();
        modelBuilder.AddUserIndexes();
        modelBuilder.AddEquipmentIndexes();
        modelBuilder.AddMuscleIndexes();
        modelBuilder.AddMovementCategoryIndexes();
        modelBuilder.AddMovementIndexes();
        modelBuilder.AddExerciseIndexes();
        modelBuilder.AddWorkoutIndexes();
    }

    public void AddLogger(IAppLogger logger)
    {
        this.logger = logger;
    }

    public void AddClock(IClock clock)
    {
        this.clock = clock;
    }
}