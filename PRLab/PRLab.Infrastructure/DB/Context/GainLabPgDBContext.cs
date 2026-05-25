

using Microsoft.EntityFrameworkCore;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Utilities;
using PRLab.Domain.Utilities.Interface;

namespace PRLab.Infrastructure.DB.Context;

//db context for postgress
public class PRLabPgDBContext(DbContextOptions< PRLabPgDBContext> options) : DbContext(options) 
{
    private IAppLogger? _logger;
    private IClock _clock;

    #region DB SETS

    public DbSet<Equipment> Equipments => Set<Equipment>();
    public DbSet<Description> Descriptors => Set<Description>();
    
    //muscles
    public DbSet<Muscle> Muscles => Set<Muscle>();
    public DbSet<MuscleAntagonist> MuscleAntagonists => Set<MuscleAntagonist>();

    //categories 
    public DbSet<MovementCategory> MovementCategories => Set<MovementCategory>();
  
    
    
    //movement
    public DbSet<Movement> Movement => Set<Movement>();
    public DbSet<MovementMuscle> MovementMuscleRelations => Set<MovementMuscle>();
    public DbSet<MovementEquipment> MovementEquipmentRelations => Set<MovementEquipment>();

    
    //user
    public DbSet<User> Users => Set<User>();


    #endregion
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        _logger?.Log("GainLabPgDBContext", "Model Creating");
        modelBuilder.HasDefaultSchema("public");
        CreateDescriptorTableModel(modelBuilder);
        CreateEquipmentTableModel(modelBuilder);
        CreateMuscleTableModel(modelBuilder);
        CreateMovementCategoryTableModel(modelBuilder);
        CreateMovementTableModel(modelBuilder);
        // CreateUserTableModel(modelBuilder);

    }

    private void CreateMovementTableModel(ModelBuilder modelBuilder)
    {
       

    }

    private void CreateMovementCategoryTableModel(ModelBuilder modelBuilder)
    {
       
     
        
    }

    private void CreateUserTableModel(ModelBuilder modelBuilder)
    {
     
    }

    private void CreateEquipmentTableModel(ModelBuilder modelBuilder)
    {
        
      
        
    }

    private void CreateMuscleTableModel(ModelBuilder modelBuilder)
    {
        
    
    }

    private void CreateDescriptorTableModel(ModelBuilder modelBuilder)
    {

    }

    public void AddLogger(IAppLogger logger)
        {
            _logger = logger;
        }

    public void AddClock(IClock clock)
    {
        _clock = clock;
    }
        
}
