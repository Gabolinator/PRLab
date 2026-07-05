using PRLab.Tools.Config;

namespace PRLab.Tools.Model;

public sealed class PRToolCommandInputData
{
    public required string[] Input { get; init; }

    public string? Command { get; init; }

    public string? Target { get; init; }

    public string? FilePath { get; init; }

    public IReadOnlyList<string> Options { get; init; } = [];

    public bool TargetIsAll =>
        !string.IsNullOrWhiteSpace(Target)
        && string.Equals(Target, SeedTargets.All, StringComparison.OrdinalIgnoreCase);

    public static PRToolCommandInputData FromInput(
        string[] args,
        PRToolConfig config)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(config);

        var options = args
            .Where(argument => argument.StartsWith("--", StringComparison.Ordinal))
            .ToList();

        var filePath = args.Length > config.FilePathIndex
                       && !args[config.FilePathIndex].StartsWith("--", StringComparison.Ordinal)
            ? args[config.FilePathIndex]
            : null;

        return new PRToolCommandInputData
        {
            Input = args,
            Command = args.Length > config.CommandIndex
                ? args[config.CommandIndex]
                : null,
            Target = args.Length > config.TargetIndex
                ? args[config.TargetIndex]
                : null,
            FilePath = filePath,
            Options = options
        };
    }

    public bool HasOption(
        params string[] optionAliases)
    {
        return Options.Any(option =>
            optionAliases.Any(alias =>
                string.Equals(option, alias, StringComparison.OrdinalIgnoreCase)));
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