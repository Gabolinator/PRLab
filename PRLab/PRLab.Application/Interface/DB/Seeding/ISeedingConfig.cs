namespace PRLab.Application.Interface.DB.Seeding;

public enum SeedingSource
{
    JsonFiles,
    Factory,
    None,
}

public interface ISeedingConfig
{
    string SeedFileDirectory { get; }
    
    SeedingSource Source { get; }
}