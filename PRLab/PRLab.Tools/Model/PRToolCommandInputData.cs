namespace PRLab.Tools.Model;

public sealed class PRToolCommandInputData
{
    public required string[] Input { get; init; }

    public string? Command { get; init; }

    public string? Target { get; init; }

    public string? FilePath { get; init; }
    
    public bool TargetIsAll => !string.IsNullOrWhiteSpace(Target) && string.Equals(Target, SeedTargets.All, StringComparison.OrdinalIgnoreCase);

    public static PRToolCommandInputData FromInput(
        string[] args,
        PRToolConfig config)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(config);

        return new PRToolCommandInputData
        {
            Input = args,
            Command = args.Length > config.CommandIndex
                ? args[config.CommandIndex]
                : null,
            Target = args.Length > config.TargetIndex
                ? args[config.TargetIndex]
                : null,
            FilePath = args.Length > config.FilePathIndex
                ? args[config.FilePathIndex]
                : null
        };
    }

    public bool IsValid()
    {
        return Input.Length > 0 && HasCommand();
    }

    public bool HasCommand()
    {
        return !string.IsNullOrWhiteSpace(Command);
    }

    public bool IsValidForSeed()
    {
        return HasCommand() && !string.IsNullOrWhiteSpace(Target);
    }

    public bool IsValidForExport()
    {
        return HasCommand()
               && !string.IsNullOrWhiteSpace(Target);
    }
}