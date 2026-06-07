

using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Utilities;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.DB.ModelBuilders;
using PRLab.Infrastructure.DB.Seeding;

namespace PRLab.Infrastructure.DB.Context;

//db context for postgress
public class PRLabPgDBContext(DbContextOptions< PRLabPgDBContext> options) : DbContext(options) 
{
    private IAppLogger? logger;
    private IClock clock;

    #region DB SETS

    public DbSet<SeedHistory> SeedHistory => Set<SeedHistory>();
    
    public DbSet<Equipment> Equipments => Set<Equipment>();
    
    //descriptions
    public DbSet<Description> Description => Set<Description>();
    public DbSet<DescriptionTranslation> DescriptionTranslations => Set<DescriptionTranslation>();
    
    //muscles
     public DbSet<Muscle> Muscles => Set<Muscle>();
     public DbSet<MuscleAntagonist> MuscleAntagonists => Set<MuscleAntagonist>();

    //categories 
    public DbSet<MovementCategory> MovementCategories => Set<MovementCategory>();
    
    //movement
     public DbSet<Movement> Movements => Set<Movement>();
    // public DbSet<MovementMuscle> MovementMuscleRelations => Set<MovementMuscle>();
    // public DbSet<MovementEquipment> MovementEquipmentRelations => Set<MovementEquipment>();

    
    //user
    public DbSet<User> Users => Set<User>();

    #endregion
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        logger?.Log("PRLabPgDBContext", "Model Creating");
        modelBuilder.HasDefaultSchema("public");
        
        modelBuilder.CreateSeedHistoryTableModel();
        
        modelBuilder.CreateUserTableModel();
        
        modelBuilder.CreateEquipmentTableModel();
        
        modelBuilder.CreateDescriptionTableModel();
        modelBuilder.CreateDescriptionTranslationTableModel();
        
        modelBuilder.CreateMuscleTableModel();
        modelBuilder.CreateMuscleAntagonistTableModel();
        
        modelBuilder.CreateMovementCategoryTableModel();
        
        modelBuilder.CreateMovementTableModel();
        modelBuilder.CreateMovementPatternTagTableModel();
        modelBuilder.CreateMovementMuscleTableModel();
        modelBuilder.CreateMovementEquipmentRequirementTableModel();
        
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
