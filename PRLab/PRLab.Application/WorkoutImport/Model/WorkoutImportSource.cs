namespace PRLab.Application.WorkoutImport.Model;

public enum WorkoutImportSourceType
{
    File = 1,
    Text = 2
}

public sealed record WorkoutImportSource
{
    public WorkoutImportSourceType Type { get; init; }

    public string? SourceReference { get; init; }

    public string? FileName { get; init; }

    public string? ContentType { get; init; }

    public byte[]? Content { get; init; }

    public string? Text { get; init; }

    private WorkoutImportSource()
    {
    }

    public static WorkoutImportSource FromFile(
        string fileName,
        byte[] content,
        string? contentType = null,
        string? sourceReference = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName);
        ArgumentNullException.ThrowIfNull(content);

        if (content.Length == 0)
        {
            throw new ArgumentException(
                "Workout import file cannot be empty.",
                nameof(content));
        }

        return new WorkoutImportSource
        {
            Type = WorkoutImportSourceType.File,
            SourceReference = sourceReference,
            FileName = fileName,
            ContentType = contentType,
            Content = content
        };
    }

    public static WorkoutImportSource FromText(
        string text,
        string? sourceReference = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(text);

        return new WorkoutImportSource
        {
            Type = WorkoutImportSourceType.Text,
            SourceReference = sourceReference,
            Text = text
        };
    }
}