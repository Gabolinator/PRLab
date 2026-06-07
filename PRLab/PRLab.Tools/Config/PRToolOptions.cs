using PRLab.Domain;
using PRLab.Tools.Model;

namespace PRLab.Tools.Config;

public sealed class PRToolOptions
{
    public ToolCommands[] Commands { get; init; } = [];

    public DomainEnum.EntityType[] Targets { get; init; } = [];

    public int CommandIndex { get; init; } = 0;

    public int TargetIndex { get; init; } = 1;

    public int FilePathIndex { get; init; } = 2;
}