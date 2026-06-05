using PRLab.Domain;
using PRLab.Infrastructure.DB.Seeding.Export;

namespace PRLab.Tools.Model;

public sealed class PRToolConfig
{
    public required IReadOnlyDictionary<string, ToolCommands> CommandAliases { get; init; }

    public required IReadOnlyDictionary<string, DomainEnum.EntityType> TargetAliases { get; init; }
    
    public required int CommandIndex { get; init; }

    public required int TargetIndex { get; init; }

    public required int FilePathIndex { get; init; }

    public static PRToolConfig Default()
    {
        ToolCommands[] defaultCommand = [ToolCommands.Seed, ToolCommands.Export];
        DomainEnum.EntityType[] availableTargets = [DomainEnum.EntityType.Equipment, DomainEnum.EntityType.MovementCategory];
        
        return new PRToolConfig
        {
            TargetAliases = SeedReferences.GetTargetAliases(availableTargets),
            CommandAliases = PRToolCommandHelper.GetCommandAliases(defaultCommand),
            CommandIndex = 0,
            TargetIndex = 1,
            FilePathIndex = 2,
        };
    }
}