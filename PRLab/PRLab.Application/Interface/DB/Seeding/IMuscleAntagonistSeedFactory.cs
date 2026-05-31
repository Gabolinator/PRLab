namespace PRLab.Application.Interface.DB.Seeding;

public interface IMuscleAntagonistSeedFactory
{
    IReadOnlyList<SeedRelationItem> CreateInitialData();
}