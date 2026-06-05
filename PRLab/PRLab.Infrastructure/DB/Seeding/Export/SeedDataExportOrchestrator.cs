using PRLab.Application.Interface.DB.Seeding.Export;
using PRLab.Domain.Utilities.Interface;

namespace PRLab.Infrastructure.DB.Seeding.Export;

public sealed class SeedDataExportOrchestrator(
    IEnumerable<ISeedDataExporter> exporters, IAppLogger logger) : ISeedDataExportOrchestrator
{
    
    private readonly IReadOnlyDictionary<string, ISeedDataExporter> exportersByTarget =
        exporters.ToDictionary(
            exporter => exporter.Target,
            exporter => exporter,
            StringComparer.OrdinalIgnoreCase);
    
    public async Task ExportAllAsync(string? filePath = null, CancellationToken ct = default)
    {
        foreach (var exporter in exportersByTarget.Values
                     .OrderBy(exporter => exporter.Target))
        {
            await exporter.ExportAsync(filePath, ct);
            logger.Log($"Exported seed from {exporter.Target} to {filePath}");
        }
    }
    
    public async Task ExportAsync(
        string target,
        string? filePath = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            throw new ArgumentException("Export target cannot be empty.", nameof(target));
        }

        var exporter = exporters
            .FirstOrDefault(exporter =>
                string.Equals(exporter.Target, target, StringComparison.OrdinalIgnoreCase));

        if (exporter is null)
        {
            var availableTargets = exporters
                .Select(exporter => exporter.Target)
                .OrderBy(exportTarget => exportTarget)
                .ToList();

            throw new InvalidOperationException(
                $"Unknown seed export target '{target}'. Available targets: {string.Join(", ", availableTargets)}.");
        }

        await exporter.ExportAsync(filePath, ct);
        logger.Log($"Exported seed from {exporter.Target} to {filePath}");
    }
    
    public IReadOnlySet<string> GetSupportedTargets()
    {
        return exportersByTarget.Keys
            .OrderBy(target => target)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
    
    public string GetDefaultFilePath(string target)
    {
        if (string.IsNullOrWhiteSpace(target))
        {
            throw new ArgumentException("Export target cannot be empty.", nameof(target));
        }

        if (!exportersByTarget.TryGetValue(target.Trim(), out var exporter))
        {
            throw new InvalidOperationException(
                $"Unknown seed export target '{target}'.");
        }

        return exporter.DefaultFilePath;
    }
}