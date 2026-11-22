using PRLab.Tools.Commands;
using PRLab.Tools.Config;

namespace PRLab.Tools.Model;

public sealed class PRToolCommandInputData
{
    public required string[] Input { get; init; }

    public string? Command { get; init; }

    public string? Target { get; init; }

    public string? FilePath { get; init; }

    public string? Profile { get; init; }

    public IReadOnlyList<string> Options { get; init; } = [];

    public bool TargetIsAll =>
        !string.IsNullOrWhiteSpace(Target)
        && string.Equals(
            Target,
            SeedTargets.All,
            StringComparison.OrdinalIgnoreCase);

    public static PRToolCommandInputData FromInput(
        string[] args,
        PRToolConfig config)
    {
        ArgumentNullException.ThrowIfNull(args);
        ArgumentNullException.ThrowIfNull(config);

        var command = args.Length > 0
            ? args[0]
            : null;

        var options = args
            .Where(argument =>
                argument.StartsWith("--", StringComparison.Ordinal))
            .ToList();

        var profile = GetOptionValue(
            args,
            "--profile");

        string? target = null;
        string? filePath = null;

        if (PRToolCommandHelper.TryGetCommand(
                command,
                config,
                out var commandValue))
        {
            switch (commandValue)
            {
                case ToolCommands.Seed:
                    target = GetPositionalArgument(
                        args,
                        1);
                    break;

                case ToolCommands.Export:
                    target = GetPositionalArgument(
                        args,
                        1);

                    filePath = GetPositionalArgument(
                        args,
                        2);
                    break;

                case ToolCommands.ImportWorkout:
                    filePath = GetPositionalArgument(
                        args,
                        1);
                    break;
            }
        }

        return new PRToolCommandInputData
        {
            Input = args,
            Command = command,
            Target = target,
            FilePath = filePath,
            Profile = profile,
            Options = options
        };
    }

    public bool HasOption(
        params string[] optionAliases)
    {
        return Options.Any(option =>
            optionAliases.Any(alias =>
                string.Equals(
                    option,
                    alias,
                    StringComparison.OrdinalIgnoreCase)));
    }

    public bool IsValid()
    {
        return Input.Length > 0
               && HasCommand();
    }

    public bool HasCommand()
    {
        return !string.IsNullOrWhiteSpace(Command);
    }

    public bool IsValidForSeed()
    {
        return HasCommand()
               && !string.IsNullOrWhiteSpace(Target);
    }

    public bool IsValidForExport()
    {
        return HasCommand()
               && !string.IsNullOrWhiteSpace(Target);
    }

    public bool IsValidForWorkoutImport()
    {
        return HasCommand()
               && !string.IsNullOrWhiteSpace(FilePath);
    }

    private static string? GetOptionValue(
        IReadOnlyList<string> args,
        string optionName)
    {
        for (var index = 0; index < args.Count; index++)
        {
            if (!string.Equals(
                    args[index],
                    optionName,
                    StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var valueIndex = index + 1;

            if (valueIndex >= args.Count ||
                args[valueIndex].StartsWith(
                    "--",
                    StringComparison.Ordinal))
            {
                return null;
            }

            return args[valueIndex];
        }

        return null;
    }

    private static string? GetPositionalArgument(
        IReadOnlyList<string> args,
        int positionalIndex)
    {
        var positionalArguments = new List<string>();

        for (var index = 0; index < args.Count; index++)
        {
            if (args[index].StartsWith(
                    "--",
                    StringComparison.Ordinal))
            {
                index++;
                continue;
            }

            positionalArguments.Add(args[index]);
        }

        return positionalArguments.Count > positionalIndex
            ? positionalArguments[positionalIndex]
            : null;
    }
}