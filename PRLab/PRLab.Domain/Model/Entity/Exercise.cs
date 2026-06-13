using PRLab.Domain.Model.Interface;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value;
using PRLab.Domain.Value.Identifier;
using PRLab.Domain.Value.Ownership;
using PRLab.Domain.Value.Update;

namespace PRLab.Domain.Model.Entity;

public sealed record Exercise : IAudited, IDescribed, IOwnedData
{
    public ExerciseId Id { get; init; }

    public string Name { get; private set; } = string.Empty;
    
    public string NameKey { get; private set; } = string.Empty;

    public Description Description { get; private set; } = null!;
    
    public OwnershipInfo Ownership { get; private set; } = null!;

    public AuditInfo Audit { get; private set; } = null!;

    private readonly List<ExerciseBlock> blocks = [];

    public IReadOnlyCollection<ExerciseBlock> Blocks => blocks
        .OrderBy(block => block.Sequence)
        .ToList();

    private Exercise()
    {
        // EF Core
    }

    private Exercise(
        ExerciseId id,
        string name,
        Description description,
        AuditInfo audit,
        OwnershipInfo ownership)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Exercise id cannot be empty.", nameof(id));
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

    public static Exercise NewBuiltIn(
        string name,
        string? description,
        User? createdBy = null)
    {
        return new Exercise(
            ExerciseId.New(),
            name,
            Description.New(description),
            AuditInfo.New(createdBy),
            OwnershipInfo.BuiltIn()
        );
    }

    public static Exercise NewBuiltIn(
        string name,
        Description description,
        User? createdBy = null)
    {
        return new Exercise(
            ExerciseId.New(),
            name,
            description,
            AuditInfo.New(createdBy),
            OwnershipInfo.BuiltIn()
        );
    }

    public static Exercise NewUserCreated(
        string name,
        string? description,
        User owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        return new Exercise(
            ExerciseId.New(),
            name,
            Description.New(description),
            AuditInfo.New(owner),
            OwnershipInfo.UserCreated(owner)
        );
    }

    public static Exercise NewUserCreated(
        string name,
        Description description,
        User owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        return new Exercise(
            ExerciseId.New(),
            name,
            description,
            AuditInfo.New(owner),
            OwnershipInfo.UserCreated(owner)
        );
    }

    public static Exercise NewImported(
        string name,
        Description description,
        User owner)
    {
        ArgumentNullException.ThrowIfNull(owner);

        return new Exercise(
            ExerciseId.New(),
            name,
            description,
            AuditInfo.New(owner),
            OwnershipInfo.Imported(owner)
        );
    }

    public static Exercise FromMovementUserCreated(
        Movement movement,
        decimal value,
        DomainEnum.WorkTargetType targetType,
        User owner,
        LoadTarget? loadTarget = null,
        RestTarget? restBetweenReps = null,
        RestTarget? transitionAfterBlock = null,
        RepExecutionDetails? executionDetails = null)
    {
        ArgumentNullException.ThrowIfNull(movement);
        ArgumentNullException.ThrowIfNull(owner);

        var exercise = new Exercise(
            ExerciseId.New(),
            movement.Name,
            movement.Description.Copy(),
            AuditInfo.New(owner),
            OwnershipInfo.UserCreated(owner)
        );

        exercise.blocks.Add(
            ExerciseBlock.New(
                exerciseId: exercise.Id,
                movementId: movement.Id,
                sequence: 1,
                target: WorkTarget.New(value, targetType),
                loadTarget: loadTarget,
                restBetweenReps: restBetweenReps,
                transitionAfterBlock: transitionAfterBlock,
                executionDetails: executionDetails
            )
        );

        return exercise;
    }
    
    public static Exercise FromMovementBuiltIn(
        Movement movement,
        decimal value,
        DomainEnum.WorkTargetType targetType,
        User? createdBy = null,
        LoadTarget? loadTarget = null,
        RestTarget? restBetweenReps = null,
        RestTarget? transitionAfterBlock = null,
        RepExecutionDetails? executionDetails = null)
    {
        ArgumentNullException.ThrowIfNull(movement);

        var exercise = new Exercise(
            ExerciseId.New(),
            movement.Name,
            movement.Description.Copy(),
            AuditInfo.New(createdBy),
            OwnershipInfo.BuiltIn()
        );

        exercise.blocks.Add(
            ExerciseBlock.New(
                exerciseId: exercise.Id,
                movementId: movement.Id,
                sequence: 1,
                target: WorkTarget.New(value, targetType),
                loadTarget: loadTarget,
                restBetweenReps: restBetweenReps,
                transitionAfterBlock: transitionAfterBlock,
                executionDetails: executionDetails
            )
        );

        return exercise;
    }
    
    private void SetName(string name)
    {
        Name = FormatingUtilities.NormalizeName(name);
        NameKey = FormatingUtilities.NormalizeNameKey(name);
    }

    public void Rename(string name, User? changedBy = null)
    {
        if (TrySetName(name))
        {
            MarkUpdated(changedBy);
        }
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

    public void Update(ExerciseUpdate update)
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
                update.Description.Language
            );

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
    }

    private void ReplaceBlocks(IReadOnlyCollection<ExerciseBlockUpdate> updatedBlocks)
    {
        blocks.Clear();

        foreach (var updatedBlock in updatedBlocks.OrderBy(block => block.Sequence))
        {
            blocks.Add(
                ExerciseBlock.New(
                    exerciseId: Id,
                    movementId: updatedBlock.MovementId,
                    sequence: updatedBlock.Sequence,
                    target: updatedBlock.Target,
                    loadTarget: updatedBlock.LoadTarget,
                    restBetweenReps: updatedBlock.RestBetweenReps,
                    transitionAfterBlock: updatedBlock.TransitionAfterBlock,
                    executionDetails: updatedBlock.ExecutionDetails
                )
            );
        }

        ResequenceBlocks();
    }
    
    public void AddBlock(
        MovementId movementId,
        decimal value,
        DomainEnum.WorkTargetType targetType,
        LoadTarget? loadTarget = null,
        RestTarget? restBetweenReps = null,
        RestTarget? transitionAfterBlock = null,
        RepExecutionDetails? executionDetails = null,
        User? changedBy = null)
    {
        var sequence = GetNextSequence();

        blocks.Add(
            ExerciseBlock.New(
                exerciseId: Id,
                movementId: movementId,
                sequence: sequence,
                target: WorkTarget.New(value, targetType),
                loadTarget: loadTarget,
                restBetweenReps: restBetweenReps,
                transitionAfterBlock: transitionAfterBlock,
                executionDetails: executionDetails
            )
        );

        MarkUpdated(changedBy);
    }

    public void RemoveBlock(ExerciseBlockId exerciseBlockId, User? changedBy = null)
    {
        var block = blocks
            .FirstOrDefault(block => block.Id == exerciseBlockId);

        if (block is null)
        {
            return;
        }

        blocks.Remove(block);
        ResequenceBlocks();
        MarkUpdated(changedBy);
    }

    public void MoveBlock(
        ExerciseBlockId exerciseBlockId,
        int newSequence,
        User? changedBy = null)
    {
        if (newSequence < 1)
        {
            throw new ArgumentException("Sequence must be greater than zero.");
        }

        var block = blocks
            .FirstOrDefault(block => block.Id == exerciseBlockId);

        if (block is null)
        {
            return;
        }

        blocks.Remove(block);

        var targetIndex = Math.Min(
            newSequence - 1,
            blocks.Count
        );

        blocks.Insert(
            targetIndex,
            block
        );

        ResequenceBlocks();

        MarkUpdated(changedBy);
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

    public void ChangeBlockTarget(
        ExerciseBlockId exerciseBlockId,
        decimal value,
        DomainEnum.WorkTargetType targetType,
        User? changedBy = null)
    {
        var block = GetBlockOrDefault(exerciseBlockId);

        if (block is null)
        {
            return;
        }

        block.ChangeTarget(WorkTarget.New(value, targetType));

        MarkUpdated(changedBy);
    }

    public void ChangeBlockLoadTarget(
        ExerciseBlockId exerciseBlockId,
        LoadTarget loadTarget,
        User? changedBy = null)
    {
        var block = GetBlockOrDefault(exerciseBlockId);

        if (block is null)
        {
            return;
        }

        block.ChangeLoadTarget(loadTarget);

        MarkUpdated(changedBy);
    }

    public void RemoveBlockLoadTarget(
        ExerciseBlockId exerciseBlockId,
        User? changedBy = null)
    {
        var block = GetBlockOrDefault(exerciseBlockId);

        if (block is null)
        {
            return;
        }

        block.RemoveLoadTarget();

        MarkUpdated(changedBy);
    }

    public void ChangeBlockRestBetweenReps(
        ExerciseBlockId exerciseBlockId,
        RestTarget restBetweenReps,
        User? changedBy = null)
    {
        var block = GetBlockOrDefault(exerciseBlockId);

        if (block is null)
        {
            return;
        }

        block.ChangeRestBetweenReps(restBetweenReps);

        MarkUpdated(changedBy);
    }

    public void RemoveBlockRestBetweenReps(
        ExerciseBlockId exerciseBlockId,
        User? changedBy = null)
    {
        var block = GetBlockOrDefault(exerciseBlockId);

        if (block is null)
        {
            return;
        }

        block.RemoveRestBetweenReps();

        MarkUpdated(changedBy);
    }

    public void ChangeBlockTransitionAfterBlock(
        ExerciseBlockId exerciseBlockId,
        RestTarget transitionAfterBlock,
        User? changedBy = null)
    {
        var block = GetBlockOrDefault(exerciseBlockId);

        if (block is null)
        {
            return;
        }

        block.ChangeTransitionAfterBlock(transitionAfterBlock);

        MarkUpdated(changedBy);
    }

    public void RemoveBlockTransitionAfterBlock(
        ExerciseBlockId exerciseBlockId,
        User? changedBy = null)
    {
        var block = GetBlockOrDefault(exerciseBlockId);

        if (block is null)
        {
            return;
        }

        block.RemoveTransitionAfterBlock();

        MarkUpdated(changedBy);
    }

    public void ChangeBlockExecutionDetails(
        ExerciseBlockId exerciseBlockId,
        RepExecutionDetails executionDetails,
        User? changedBy = null)
    {
        var block = GetBlockOrDefault(exerciseBlockId);

        if (block is null)
        {
            return;
        }

        block.ChangeExecutionDetails(executionDetails);

        MarkUpdated(changedBy);
    }

    public void RemoveBlockExecutionDetails(
        ExerciseBlockId exerciseBlockId,
        User? changedBy = null)
    {
        var block = GetBlockOrDefault(exerciseBlockId);

        if (block is null)
        {
            return;
        }

        block.RemoveExecutionDetails();

        MarkUpdated(changedBy);
    }

    private ExerciseBlock? GetBlockOrDefault(ExerciseBlockId exerciseBlockId)
    {
        return blocks
            .FirstOrDefault(block => block.Id == exerciseBlockId);
    }

    private int GetNextSequence()
    {
        if (blocks.Count == 0)
        {
            return 1;
        }

        return blocks.Max(block => block.Sequence) + 1;
    }

    private void ResequenceBlocks()
    {
        for (var index = 0; index < blocks.Count; index++)
        {
            blocks[index].ChangeSequence(index + 1);
        }
    }
}