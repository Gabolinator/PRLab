using System.Text.Json;
using System.Text.Json.Serialization;
using PRLab.Application.Interface.DB;
using PRLab.Application.Interface.DB.Seeding;
using PRLab.Application.Interface.DB.Seeding.Factory;
using PRLab.Application.Models.DB.Seeding;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Enum.System;

namespace PRLab.Infrastructure.DB.Seeding.FromJson.Factory;

public abstract class BaseJsonSeedFactory<TEntity, TJsonDto>(
    IUserService userService,
    ISeedingConfig config) : IJsonSeedFactory<TEntity, TJsonDto>
{
    public string SeedFileName =>
        SeedFileNames.GetSeedFileNameForEntity(Entity);
    
    protected User SeedUser =>
        userService.GetSystemAdminUser("Seed");
    
   private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
        Converters =
        {
            new JsonStringEnumConverter()
        }
    };
    
    protected abstract EntityType Entity { get; }

    private string SeedFilePath => SeedFilePathBuilder.Build(
        config.SeedFileDirectory,
        SeedFileName);

    public IReadOnlyList<TJsonDto> LoadSeedDtos()
    {
        if (!File.Exists(SeedFilePath))
        {
            throw new FileNotFoundException(
                $"Seed file was not found at '{SeedFilePath}'.",
                SeedFilePath);
        }

        var json = File.ReadAllText(SeedFilePath);

        var seedDtos = JsonSerializer.Deserialize<IReadOnlyList<TJsonDto>>(
            json,
            JsonOptions);

        return seedDtos ?? [];
    }

    public IReadOnlyList<SeedItem<TEntity>> CreateSeedItems()
    {
        return LoadSeedDtos()
            .Select(ToSeedItem)
            .ToList();
    }
    
    public abstract SeedItem<TEntity> ToSeedItem(TJsonDto seedDto);
}