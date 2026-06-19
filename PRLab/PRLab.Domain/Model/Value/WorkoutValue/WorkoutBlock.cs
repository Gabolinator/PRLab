using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Interface;
using PRLab.Domain.Model.Value.Enum.Workout;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Ownership;
using PRLab.Domain.Model.Value.Prescription;
using PRLab.Domain.Utilities;

namespace PRLab.Domain.Model.Value.WorkoutValue;

public sealed record WorkoutBlock : IAudited, IOwnedData
{
    public WorkoutBlockId Id { get; init; }

    public string Name { get; private set; } = string.Empty;

    public string NameKey { get; private set; } = string.Empty;

    public WorkoutBlockType BlockType { get; private set; }

    // how many times do we do the whole block ? 
    public BlockRepeatPrescription BlockRepeatPrescription { get; private set; }
   
    public AuditInfo Audit { get; private set; } = null!;

    public OwnershipInfo Ownership { get; private set; } = null!;

    private readonly List<WorkoutBlockSegment> segments = [];

    public IReadOnlyCollection<WorkoutBlockSegment> Segments => segments
        .OrderBy(segment => segment.Sequence)
        .ToList();

    private WorkoutBlock()
    {
        // EF Core
    }

    private WorkoutBlock(
        WorkoutBlockId id,
        string name,
        WorkoutBlockType blockType,
        BlockRepeatPrescription blockRepeatPrescription,
        AuditInfo audit,
        OwnershipInfo ownership)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Workout block id cannot be empty.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(blockRepeatPrescription);
        ArgumentNullException.ThrowIfNull(audit);
        ArgumentNullException.ThrowIfNull(ownership);

        Id = id;
        SetName(name);
        BlockType = blockType;
        BlockRepeatPrescription = blockRepeatPrescription;
        Audit = audit;
        Ownership = ownership;
    }

    public static WorkoutBlock NewBuiltIn(
        string name,
        WorkoutBlockType blockType,
        BlockRepeatPrescription? roundPrescription = null,
        User? createdBy = null)
    {
        return new WorkoutBlock(
            WorkoutBlockId.New(),
            name,
            blockType,
            roundPrescription ?? BlockRepeatPrescription.Once(),
            AuditInfo.New(createdBy),
            OwnershipInfo.BuiltIn());
    }

    public static WorkoutBlock NewUserCreated(
        string name,
        WorkoutBlockType blockType,
        User owner,
        BlockRepeatPrescription? roundPrescription = null)
    {
        ArgumentNullException.ThrowIfNull(owner);

        return new WorkoutBlock(
            WorkoutBlockId.New(),
            name,
            blockType,
            roundPrescription ?? BlockRepeatPrescription.Once(),
            AuditInfo.New(owner),
            OwnershipInfo.UserCreated(owner));
    }

    public static WorkoutBlock NewImported(
        string name,
        WorkoutBlockType blockType,
        User owner,
        BlockRepeatPrescription? roundPrescription = null)
    {
        ArgumentNullException.ThrowIfNull(owner);

        return new WorkoutBlock(
            WorkoutBlockId.New(),
            name,
            blockType,
            roundPrescription ?? BlockRepeatPrescription.Once(),
            AuditInfo.New(owner),
            OwnershipInfo.Imported(owner));
    }

    public void Rename(string name, User? changedBy = null)
    {
        if (TrySetName(name))
        {
            MarkUpdated(changedBy);
        }
    }

    public void ChangeBlockType(
        WorkoutBlockType blockType,
        User? changedBy = null)
    {
        if (BlockType == blockType)
        {
            return;
        }

        BlockType = blockType;
        MarkUpdated(changedBy);
    }

    public void ChangeRoundPrescription(
        BlockRepeatPrescription blockRepeatPrescription,
        User? changedBy = null)
    {
        ArgumentNullException.ThrowIfNull(blockRepeatPrescription);

        if (BlockRepeatPrescription == blockRepeatPrescription)
        {
            return;
        }

        BlockRepeatPrescription = blockRepeatPrescription;
        MarkUpdated(changedBy);
    }

    public void AddSegment(
        WorkoutBlockSegment segment,
        User? changedBy = null,
        int? atSequence = null)
    {
        ArgumentNullException.ThrowIfNull(segment);

        var targetSequence = NormalizeInsertSequence(atSequence ?? segment.Sequence);

        ShiftSegmentsFromSequence(targetSequence);

        segment.ChangeSequence(targetSequence);

        segments.Add(segment);

        MarkUpdated(changedBy);
    }

    public void RemoveSegment(
        WorkoutBlockSegmentId segmentId,
        User? changedBy = null)
    {
        var segment = segments
            .FirstOrDefault(existingSegment => existingSegment.Id == segmentId);

        if (segment is null)
        {
            return;
        }

        segments.Remove(segment);
        ResequenceSegments();
        MarkUpdated(changedBy);
    }

    public void MoveSegment(
        WorkoutBlockSegmentId segmentId,
        int newSequence,
        User? changedBy = null)
    {
        if (newSequence < 1)
        {
            throw new ArgumentException("Sequence must be greater than zero.", nameof(newSequence));
        }

        var segment = segments
            .FirstOrDefault(existingSegment => existingSegment.Id == segmentId);

        if (segment is null)
        {
            return;
        }

        segments.Remove(segment);

        var targetIndex = Math.Min(
            newSequence - 1,
            segments.Count);

        segments.Insert(
            targetIndex,
            segment);

        ResequenceSegments();

        MarkUpdated(changedBy);
    }

    public void MarkUpdated(User? changedBy = null)
    {
        Audit = Audit.MarkUpdated(changedBy);
    }

    public void MarkDeleted(User? deletedBy = null)
    {
        Audit = Audit.MarkDeleted(deletedBy);
    }

    void IAudited.MarkUpdated(User? changedBy)
    {
        MarkUpdated(changedBy);
    }

    void IAudited.MarkDeleted(User? deletedBy)
    {
        MarkDeleted(deletedBy);
    }

    private void SetName(string name)
    {
        Name = FormatingUtilities.NormalizeName(name);
        NameKey = FormatingUtilities.NormalizeNameKey(name);
    }

    private bool TrySetName(string name)
    {
        var normalizedName = FormatingUtilities.NormalizeName(name);
        var normalizedNameKey = FormatingUtilities.NormalizeNameKey(name);

        if (Name == normalizedName && NameKey == normalizedNameKey)
        {
            return false;
        }

        Name = normalizedName;
        NameKey = normalizedNameKey;

        return true;
    }

    private int GetNextSequence()
    {
        if (segments.Count == 0)
        {
            return 1;
        }

        return segments.Max(segment => segment.Sequence) + 1;
    }

    private int NormalizeInsertSequence(int? atSequence)
    {
        var nextSequence = GetNextSequence();

        if (atSequence is null)
        {
            return nextSequence;
        }

        if (atSequence <= 0)
        {
            throw new ArgumentException(
                "Segment sequence must be greater than zero.",
                nameof(atSequence));
        }

        if (atSequence > segments.Count)
        {
            return nextSequence;
        }

        return atSequence.Value;
    }

    private void ShiftSegmentsFromSequence(int sequence)
    {
        foreach (var segment in segments
                     .Where(existingSegment => existingSegment.Sequence >= sequence)
                     .OrderByDescending(existingSegment => existingSegment.Sequence))
        {
            segment.ChangeSequence(segment.Sequence + 1);
        }
    }

    private void ResequenceSegments()
    {
        var orderedSegments = segments
            .OrderBy(segment => segment.Sequence)
            .ToList();

        for (var index = 0; index < orderedSegments.Count; index++)
        {
            orderedSegments[index].ChangeSequence(index + 1);
        }
    }
}