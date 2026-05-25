using PRLab.Model.Interface;
using PRLab.Value.Identifier;

namespace PRLab.Model.Entity;

public sealed record Exercise : IAudited
{
    public ExerciseId Id { get; init; }

    public string Name { get; private set; } = string.Empty;
    
    public AuditInfo Audit { get; private set; }
}