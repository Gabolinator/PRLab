namespace PRLab.Application.Interface.DB.Seeding.Export;

public interface ISeedDataExporter
{
    public string Target { get; }
    
    string DefaultFilePath { get; }
    
    Task ExportAsync(string filePath, CancellationToken ct = default);
}