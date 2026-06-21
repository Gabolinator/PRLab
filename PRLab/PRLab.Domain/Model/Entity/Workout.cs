using PRLab.Domain.Model.Interface;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Model.Value;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Ownership;
using PRLab.Domain.Model.Value.Prescription;
using PRLab.Domain.Model.Value.Prescription.Common;
using PRLab.Domain.Model.Value.Update;
using PRLab.Domain.Utilities;

namespace PRLab.Domain.Model.Entity;

public sealed record Workout : IAudited, IDescribed, IOwnedData
{
    public WorkoutId Id { get; init; }

    public string Name { get; private set; } = string.Empty;

    public string NameKey { get; private set; } = string.Empty;
    
    public EstimatedDuration? EstimatedDuration { get; private set; } //de we compute it from the child ? 

    public Description Description { get; private set; } = null!;

    public OwnershipInfo Ownership { get; private set; } = null!;

    public AuditInfo Audit { get; private set; } = null!;

    private readonly List<WorkoutBlockAssignment> blocks = [];

    public IReadOnlyCollection<WorkoutBlockAssignment> Blocks => blocks
        .OrderBy(block => block.Sequence)
        .ToList();

    private Workout()
    {
        // EF Core
    }

    private Workout(
        WorkoutId id,
        string name,
        Description description,
        AuditInfo audit,
        OwnershipInfo ownership)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Workout id cannot be empty.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(description);
        ArgumentNullException.ThrowIfNull(audit);
        ArgumentNullException.ThrowIfNull(ownership);

        Id = id;
        SetName(name);
        Description = description;
        Audit = audit;
        Ownership = ownership;
    }

    public static Workout NewBuiltIn(
        string name,
        string? description,
        User? createdBy = null)
    {
        return new Workout(
            WorkoutId.New(),
            name,
            Description.New(description),
            AuditInfo.New(createdBy),
            OwnershipInfo.BuiltIn());
    }

    public static Workout NewBuiltIn(
        string name,
        Description description,
        User? createdBy = null)
    {
        return new Workout(
            WorkoutId.New(),
            name,
            description,
            AuditInfo.New(createdBy),
            OwnershipInfo.BuiltIn());
    }
    
    public static Workout NewBuiltInWithId(
        WorkoutId id,
        string name,
        Description description,
        User createdBy)
    {
        return new Workout(
            id,
            name,
            description,
            AuditInfo.New(createdBy),
            OwnershipInfo.BuiltIn());
    }

    public static Workout NewUserCreated(
        string name,
        string? description,
        User owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        return new Workout(
            WorkoutId.New(),
            name,
            Description.New(description),
            AuditInfo.New(owner),
            OwnershipInfo.UserCreated(owner));
    }

    public static Workout NewUserCreated(
        string name,
        Description description,
        User owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        return new Workout(
            WorkoutId.New(),
            name,
            description,
            AuditInfo.New(owner),
            OwnershipInfo.UserCreated(owner));
    }

    public static Workout NewImported(
        string name,
        Description description,
        User owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        return new Workout(
            WorkoutId.New(),
            name,
            description,
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

    public void ChangeDescription(
        string? content,
        LocalizationHelper.Language? languageCode,
        User? changedBy = null)
    {
        Description = Description.ChangeContent(content, languageCode);
        MarkUpdated(changedBy);
    }

    public void RemoveDescription(
        LocalizationHelper.Language? languageCode,
        User? changedBy = null)
    {
        Description = Description.RemoveContent(languageCode);
        MarkUpdated(changedBy);
    }

    public void AddBlock(
        WorkoutBlockAssignment block,
        User? changedBy = null,
        int? atSequence = null)
    {
        ArgumentNullException.ThrowIfNull(block);

        var targetSequence = NormalizeInsertSequence(atSequence ?? block.Sequence);

        ShiftBlocksFromSequence(targetSequence);

        block.ChangeSequence(targetSequence);

        blocks.Add(block);

        MarkUpdated(changedBy);
    }

    public void RemoveBlock(
        WorkoutBlockAssignmentId blockAssignmentId,
        User? changedBy = null)
    {
        var block = blocks
            .FirstOrDefault(existingBlock => existingBlock.Id == blockAssignmentId);

        if (block is null)
        {
            return;
        }

        blocks.Remove(block);
        ResequenceBlocks();
        MarkUpdated(changedBy);
    }

    public void MoveBlock(
        WorkoutBlockAssignmentId blockAssignmentId,
        int newSequence,
        User? changedBy = null)
    {
        if (newSequence < 1)
        {
            throw new ArgumentException("Sequence must be greater than zero.", nameof(newSequence));
        }

        var block = blocks
            .FirstOrDefault(existingBlock => existingBlock.Id == blockAssignmentId);

        if (block is null)
        {
            return;
        }

        blocks.Remove(block);

        var targetIndex = Math.Min(
            newSequence - 1,
            blocks.Count);

        blocks.Insert(
            targetIndex,
            block);

        ResequenceBlocks();

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
        if (blocks.Count == 0)
        {
            return 1;
        }

        return blocks.Max(block => block.Sequence) + 1;
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
                "Block sequence must be greater than zero.",
                nameof(atSequence));
        }

        if (atSequence > blocks.Count)
        {
            return nextSequence;
        }

        return atSequence.Value;
    }

    private void ShiftBlocksFromSequence(int sequence)
    {
        foreach (var block in blocks
                     .Where(existingBlock => existingBlock.Sequence >= sequence)
                     .OrderByDescending(existingBlock => existingBlock.Sequence))
        {
            block.ChangeSequence(block.Sequence + 1);
        }
    }

    private void ResequenceBlocks()
    {
        var orderedBlocks = blocks
            .OrderBy(block => block.Sequence)
            .ToList();

        for (var index = 0; index < orderedBlocks.Count; index++)
        {
            orderedBlocks[index].ChangeSequence(index + 1);
        }
    }
    
    public void ChangeEstimatedDuration(
        EstimatedDuration estimatedDuration,
        User? changedBy = null)
    {
        ArgumentNullException.ThrowIfNull(estimatedDuration);

        if (EstimatedDuration == estimatedDuration)
        {
            return;
        }

        EstimatedDuration = estimatedDuration;
        MarkUpdated(changedBy);
    }

    public void RemoveEstimatedDuration(User? changedBy = null)
    {
        if (EstimatedDuration is null)
        {
            return;
        }

        EstimatedDuration = null;
        MarkUpdated(changedBy);
    }

    public void RecalculateEstimatedDuration(User? changedBy = null)
    {
        var estimatedDuration = WorkoutDurationCalculator.EstimateBlockDuration(this);

        if (estimatedDuration is null || EstimatedDuration == estimatedDuration)
        {
            return;
        }

        EstimatedDuration = estimatedDuration;
        MarkUpdated(changedBy);
    }

    public bool Update(WorkoutUpdate update)
    {
        ArgumentNullException.ThrowIfNull(update);

        var hasChanged = false;

        if (!string.IsNullOrWhiteSpace(update.Name))
        {
            hasChanged = TrySetName(update.Name) || hasChanged;
        }

        if (update.Description is not null)
        {
            Description = Description.ChangeContent(
                update.Description.Content,
                update.Description.Language);

            hasChanged = true;
        }

        if (update.WasEstimatedDurationProvided)
        {
            if (update.EstimatedDuration is null)
            {
                RemoveEstimatedDuration();
            }
            else
            {
                ChangeEstimatedDuration(update.EstimatedDuration);
            }

            hasChanged = true;
        }

        if (update.Blocks is not null)
        {
            ReplaceBlocks(update.Blocks);
            hasChanged = true;
        }

        if (hasChanged)
        {
            MarkUpdated(update.UpdatedBy);
        }

        return hasChanged;
    }
    

    private void ReplaceBlocks(
        IReadOnlyCollection<WorkoutBlockAssignmentUpdate> updates)
    {
        ArgumentNullException.ThrowIfNull(updates);

        ValidateBlockUpdates(updates);

        blocks.Clear();

        foreach (var update in updates.OrderBy(block => block.Sequence))
        {
            blocks.Add(
                WorkoutBlockAssignment.New(
                    workoutId: Id,
                    workoutBlock: update.WorkoutBlock,
                    sequence: update.Sequence));
        }

        ResequenceBlocks();
    }

    private static void ValidateBlockUpdates(
        IReadOnlyCollection<WorkoutBlockAssignmentUpdate> updates)
    {
        foreach (var update in updates)
        {
            ArgumentNullException.ThrowIfNull(update);
            ArgumentNullException.ThrowIfNull(update.WorkoutBlock);

            if (update.Sequence < 1)
            {
                throw new ArgumentException(
                    "Workout block assignment sequence must be greater than zero.",
                    nameof(updates));
            }
        }

        var duplicateSequences = updates
            .GroupBy(update => update.Sequence)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key)
            .ToList();

        if (duplicateSequences.Count > 0)
        {
            throw new ArgumentException(
                $"Workout block assignment sequence contains duplicate value(s): {string.Join(", ", duplicateSequences)}.",
                nameof(updates));
        }
    }
}