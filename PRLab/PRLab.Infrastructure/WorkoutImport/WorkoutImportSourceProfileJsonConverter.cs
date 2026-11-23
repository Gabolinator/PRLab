using System.Text.Json;
using System.Text.Json.Serialization;
using PRLab.Application.WorkoutImport.Model;
using PRLab.Infrastructure.WorkoutImport.Extractor.Excel;

namespace PRLab.Infrastructure.WorkoutImport;

public sealed class WorkoutImportSourceProfileJsonConverter
    : JsonConverter<WorkoutImportSourceProfile>
{
    public override WorkoutImportSourceProfile? Read(
        ref Utf8JsonReader reader,
        Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);

        var root = document.RootElement;

        if (!root.TryGetProperty(
                "Type",
                out var typeProperty))
        {
            throw new JsonException(
                "Workout import source profile requires a 'Type' property.");
        }

        var type = typeProperty.GetString();

        return type switch
        {
            "Excel" => JsonSerializer.Deserialize<ExcelWorkoutImportSourceProfile>(
                root.GetRawText(),
                options),

            _ => throw new JsonException(
                $"Unsupported workout import source profile type '{type}'.")
        };
    }

    public override void Write(
        Utf8JsonWriter writer,
        WorkoutImportSourceProfile value,
        JsonSerializerOptions options)
    {
        switch (value)
        {
            case ExcelWorkoutImportSourceProfile excelProfile:
                JsonSerializer.Serialize(
                    writer,
                    excelProfile,
                    options);
                break;

            default:
                throw new JsonException(
                    $"Unsupported workout import source profile type " +
                    $"'{value.GetType().Name}'.");
        }
    }
}