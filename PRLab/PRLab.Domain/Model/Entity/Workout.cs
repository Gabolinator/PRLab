using PRLab.Domain.Model.Interface;
using PRLab.Domain.Model.Value;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Ownership;
using PRLab.Domain.Utilities;

namespace PRLab.Domain.Model.Entity;

public class Workout : IAudited, IDescribed, IOwnedData
{
    public WorkoutId Id { get; init; }

    public string Name { get; private set; } = string.Empty;
    
    public string NameKey { get; private set; } = string.Empty;

    public Description Description { get; private set; } = null!;
    
    public OwnershipInfo Ownership { get; private set; } = null!;

    public AuditInfo Audit { get; private set; } = null!;
    
    private readonly List<WorkoutBlock> blocks = [];

    public IReadOnlyCollection<WorkoutBlock> Blocks => blocks
        .OrderBy(block => block.Sequence)
        .ToList();
    
    public void ChangeDescription(string? content, LocalizationHelper.Language? languageCode, User? changedBy = null)
    {
        throw new NotImplementedException();
    }

    public void RemoveDescription(LocalizationHelper.Language? languageCode, User? changedBy = null)
    {
        throw new NotImplementedException();
    }

  
    public void MarkUpdated(User? changedBy = null)
    {
        throw new NotImplementedException();
    }

    public void MarkDeleted(User? deletedBy = null)
    {
        throw new NotImplementedException();
    }

    private Workout()
    {
        // EF Core
    }
    
    
}