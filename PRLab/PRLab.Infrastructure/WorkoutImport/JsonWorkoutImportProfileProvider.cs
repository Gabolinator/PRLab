using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using PRLab.Application.WorkoutImport;
using PRLab.Application.WorkoutImport.Config;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Domain.Utilities.Interface;

namespace PRLab.Infrastructure.WorkoutImport;

public sealed class JsonWorkoutImportProfileProvider(
    IOptions<WorkoutImportOptions> options,
    IAppLogger logger)
    : IWorkoutImportProfileProvider
{
    private readonly WorkoutImportOptions options = options.Value;

    public async Task<WorkoutImportProfile> GetAsync(
        string? profileName = null,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(profileName))
        {
            logger.LogWarning(nameof(JsonWorkoutImportProfileProvider), "No workout import profile was specified.");
        }

        var resolvedProfile = string.IsNullOrWhiteSpace(profileName)
            ? options.DefaultProfile
            : profileName;

        if (string.IsNullOrWhiteSpace(resolvedProfile))
        {
            throw new InvalidOperationException(
                "No workout import profile was specified.");
        }

        logger.Log(
            nameof(JsonWorkoutImportProfileProvider),
            $"Getting workout import profile {resolvedProfile}");

        var filePath = Path.Combine(
            AppContext.BaseDirectory,
            options.ProfileDirectory,
            $"{resolvedProfile}.json");

        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException(
                $"Workout import profile '{resolvedProfile}' was not found.",
                filePath);
        }

        var json = await File.ReadAllTextAsync(
            filePath,
            ct);

        var serializerOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        serializerOptions.Converters.Add(
            new JsonStringEnumConverter());

        serializerOptions.Converters.Add(
            new WorkoutImportSourceProfileJsonConverter());

        var profile = JsonSerializer.Deserialize<WorkoutImportProfile>(
            json,
            serializerOptions);

        if (profile == null)
        {
            logger.LogWarning(nameof(JsonWorkoutImportProfileProvider), $"Could not deserialize workout import profile. {resolvedProfile}");
        }

        else
        {
            logger.Log(nameof(JsonWorkoutImportProfileProvider), $"Found  workout import profile: {profile.Name}");
        }
        
        return profile
               ?? throw new InvalidOperationException(
                   $"Workout import profile '{resolvedProfile}' could not be deserialized.");
    }
}