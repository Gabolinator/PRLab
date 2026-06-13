using System.Text.Json;
using System.Text.Json.Serialization;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Export;
using PRLab.Domain;
using PRLab.Domain.Value.Enum.System;
using PRLab.Infrastructure.DB.Seeding.FromJson;

namespace PRLab.Infrastructure.DB.Seeding.Export.Exporter;

public abstract class BaseJsonSeedDataExporter<TSeedDto>(
    ISeedingConfig config) : ISeedDataExporter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };
    
    public string SeedFileName => SeedFileNames.GetSeedFileNameForEntity(Entity);

    public string Target => SeedReferences.GetEntityAlias(Entity);
    
    public abstract EntityType Entity { get;}
    
    public string DefaultFilePath => SeedFilePathBuilder.Build(
        config.SeedFileDirectory,
        SeedFileName);

    public async Task ExportAsync(
        string? specifiedFilePath = null,
        CancellationToken ct = default)
    {
        var filePath = specifiedFilePath ?? DefaultFilePath;

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("Export file path cannot be empty.", nameof(specifiedFilePath));
        }

        var seedDtos = await CreateSeedDtosAsync(ct);

        var directory = Path.GetDirectoryName(filePath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        var json = JsonSerializer.Serialize(seedDtos, JsonOptions);

        await File.WriteAllTextAsync(filePath, json, ct);
    }

    protected abstract Task<IReadOnlyList<TSeedDto>> CreateSeedDtosAsync(CancellationToken ct);
}