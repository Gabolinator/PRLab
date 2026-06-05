namespace PRLab.Application.Interface.DB.Seeding.Export;

public interface ISeedDataExportOrchestrator
{
    Task ExportAsync(
        string target,
        string? filePath = null,
        CancellationToken ct = default);

    public IReadOnlySet<string> GetSupportedTargets();
    
    Task ExportAllAsync(string? filePath = null, CancellationToken ct = default);
    
    string GetDefaultFilePath(string target);
    
}