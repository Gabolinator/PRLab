using PRLab.Domain;
using PRLab.Infrastructure.DB.Seeding.Export;
using PRLab.Tools.Config;

namespace PRLab.Tools.Model;

public sealed class PRToolConfig
{
    public required IReadOnlyDictionary<string, ToolCommands> CommandAliases { get; init; }

    public required IReadOnlyDictionary<string, DomainEnum.EntityType> TargetAliases { get; init; }

    public required int CommandIndex { get; init; }

    public required int TargetIndex { get; init; }

    public required int FilePathIndex { get; init; }

    public static PRToolConfig FromOptions(PRToolOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var commands = options.Commands.Length == 0
            ? [ToolCommands.Seed, ToolCommands.Export]
            : options.Commands;

        var targets = options.Targets.Length == 0
            ? [DomainEnum.EntityType.Equipment, DomainEnum.EntityType.MovementCategory, DomainEnum.EntityType.Muscle]
            : options.Targets;

        return new PRToolConfig
        {
            TargetAliases = SeedReferences.GetTargetAliases(targets),
            CommandAliases = PRToolCommandHelper.GetCommandAliases(commands),
            CommandIndex = options.CommandIndex,
            TargetIndex = options.TargetIndex,
            FilePathIndex = options.FilePathIndex,
        };
    }
}