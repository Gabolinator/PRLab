using PRLab.Domain.Model.Interface;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Domain.Model.Entity;

public sealed record Exercise : IAudited, IDescribed
{
    public ExerciseId Id { get; init; }

    public string Name { get; private set; } = string.Empty;
    
    public string NameKey { get; private set; } = string.Empty;

    public Description Description { get; private set; } = null!;

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
        AuditInfo audit)
    {
        Id = id;
        SetName(name);
        Description = description;
        Audit = audit;
    }

    public static Exercise New(
        string name,
        string? description,
        User? createdBy = null)
    {
        return new Exercise(
            ExerciseId.New(),
            name,
            Description.New(description),
            AuditInfo.New(createdBy)
        );
    }

    public static Exercise FromMovement(
        Movement movement,
        decimal value,
        DomainEnum.WorkTargetType targetType,
        LoadTarget? loadTarget = null,
        RestTarget? restBetweenReps = null,
        RestTarget? transitionAfterBlock = null,
        RepExecutionDetails? executionDetails = null,
        User? createdBy = null)
    {
        ArgumentNullException.ThrowIfNull(movement);

        var exercise = new Exercise(
            ExerciseId.New(),
            movement.Name,
            movement.Description.Copy(),
            AuditInfo.New(createdBy)
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
        SetName(name);
        MarkUpdated(changedBy);
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