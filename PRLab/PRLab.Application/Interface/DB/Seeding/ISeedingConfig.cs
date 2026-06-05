namespace PRLab.Application.Interface.DB.Seeding;

public interface ISeedingConfig
{
    string SeedFileDirectory { get; }
    
    bool SeedFromFile { get; }
}