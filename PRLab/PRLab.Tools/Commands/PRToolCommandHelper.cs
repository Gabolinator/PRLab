using PRLab.Tools.Model;

namespace PRLab.Tools.Commands;

public enum ToolCommands
{
    Seed,
    Export,
    ImportWorkout
}

public static class PRToolCommandHelper
{
    public static readonly IReadOnlyDictionary<ToolCommands, string> CommandAliases =
        new Dictionary<ToolCommands, string>
        {
            [ToolCommands.Seed] = "seed",
            [ToolCommands.Export] = "export-seed",
            [ToolCommands.ImportWorkout] = "import-workout"
        };

    public static IReadOnlyDictionary<string, ToolCommands> GetCommandAliases(
        IReadOnlyCollection<ToolCommands> availableCommands)
    {
        var aliases = new Dictionary<string, ToolCommands>(
            StringComparer.OrdinalIgnoreCase);

        foreach (var command in availableCommands)
        {
            if (!CommandAliases.TryGetValue(
                    command,
                    out var commandAlias))
            {
                continue;
            }

            aliases[commandAlias] = command;
        }

        return aliases;
    }

    public static bool TryGetCommand(
        string? command,
        PRToolConfig config,
        out ToolCommands commandValue)
    {
        commandValue = default;

        if (string.IsNullOrWhiteSpace(command))
        {
            return false;
        }

        return config.CommandAliases.TryGetValue(
            command.Trim(),
            out commandValue);
    }
}