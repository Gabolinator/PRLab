using PRLab.Domain.Model.Interface;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Domain.Model.Entity;

public sealed record Muscle : IAudited, IDescribed
{
    public MuscleId Id { get; init; }

    public string Name { get; private set; } = string.Empty;

    public DomainEnum.BodySection BodySection { get; private set; }

    public Description Description { get; private set; } = null!;

    public AuditInfo Audit { get; private set; } = null!;

    private readonly List<MuscleAntagonist> antagonists = [];

    public IReadOnlyCollection<MuscleAntagonist> Antagonists => antagonists;

    private HashSet<MuscleId> AntagonistIDs => Antagonists
        .Select(antagonist => antagonist.AntagonistMuscleId)
        .ToHashSet();

    private Muscle()
    {
        // EF Core
    }

    private Muscle(
        MuscleId id,
        string name,
        DomainEnum.BodySection bodySection,
        Description description,
        AuditInfo audit)
    {
        Id = id;
        Name = FormatingUtilities.NormalizeName(name);
        BodySection = bodySection;
        Description = description;
        Audit = audit;
    }

    public static Muscle New(
        string name,
        DomainEnum.BodySection bodySection,
        string? description,
        User? createdBy = null)
    {
        return new Muscle(
            MuscleId.New(),
            name,
            bodySection,
            Description.New(description),
            AuditInfo.New(createdBy)
        );
    }

    public void ChangeBodySection(
        DomainEnum.BodySection bodySection,
        User? changedBy = null)
    {
        BodySection = bodySection;
        MarkUpdated(changedBy);
    }

    public void Rename(string name, User? changedBy = null)
    {
        Name = FormatingUtilities.NormalizeName(name);
        MarkUpdated(changedBy);
    }

    public void AddAntagonist(MuscleId antagonistMuscleId, User? changedBy = null)
    {
        if (antagonistMuscleId == Id)
        {
            throw new ArgumentException("A muscle cannot be its own antagonist.");
        }

        if (AntagonistIDs.Contains(antagonistMuscleId))
        {
            return;
        }

        antagonists.Add(
            MuscleAntagonist.New(Id, antagonistMuscleId)
        );

        MarkUpdated(changedBy);
    }

    public void RemoveAntagonist(MuscleId antagonistMuscleId, User? changedBy = null)
    {
        if (!AntagonistIDs.Contains(antagonistMuscleId))
        {
            return;
        }

        var relation = antagonists
            .FirstOrDefault(antagonist => antagonist.AntagonistMuscleId == antagonistMuscleId);

        if (relation is null)
        {
            return;
        }

        antagonists.Remove(relation);
        MarkUpdated(changedBy);
    }
    
    public void ChangeDescription(
        string? content,
        string languageCode = "en",
        User? changedBy = null)
    {
        Description = Description.ChangeContent(content, languageCode);
        MarkUpdated(changedBy);
    }

    public void RemoveDescription(
        string languageCode = "en",
        User? changedBy = null)
    {
        Description = Description.RemoveContent(languageCode);
        MarkUpdated(changedBy);
    }
    
    void IAudited.MarkUpdated(User? changedBy)
    {
        MarkUpdated(changedBy);
    }

    void IAudited.MarkDeleted(User? deletedBy)
    {
        MarkDeleted(deletedBy);
    }
    
    private void MarkUpdated(User? changedBy = null)
    {
        Audit = Audit.MarkUpdated(changedBy);
    }

    private void MarkDeleted(User? deletedBy = null)
    {
        Audit = Audit.MarkDeleted(deletedBy);
    }
}