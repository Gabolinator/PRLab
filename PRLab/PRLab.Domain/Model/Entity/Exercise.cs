using PRLab.Domain.Model.Interface;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Domain.Model.Entity;

public sealed record Exercise : IAudited, IDescribed
{
    public ExerciseId Id { get; init; }

    public string Name { get; private set; } = string.Empty;

    public Description Description { get; private set; } = null!;

    public AuditInfo Audit { get; private set; } = null!;

    private readonly List<ExerciseBlock> blocks = [];

    public IReadOnlyCollection<ExerciseBlock> Blocks => blocks
        .OrderBy(block => block.Sequence)
        .ToList();

    private HashSet<MovementId> MovementIDs => Blocks
        .Select(block => block.MovementId)
        .ToHashSet();

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
        Name = FormatingUtilities.NormalizeName(name);
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
        DomainEnum.RepType repType,
        RepExecutionDetails? executionDetails = null,
        User? createdBy = null)
    {
        var exercise = new Exercise(
            ExerciseId.New(),
            movement.Name,
            movement.Description.Copy(),
            AuditInfo.New(createdBy)
        );

        exercise.blocks.Add(
            ExerciseBlock.New(
                exercise.Id,
                movement.Id,
                1,
                WorkTarget.New(value, repType),
                executionDetails
            )
        );

        return exercise;
    }
    
    public void Rename(string name, User? changedBy = null)
    {
        Name = FormatingUtilities.NormalizeName(name);
        MarkUpdated(changedBy);
    }

    public void AddBlock(
        MovementId movementId,
        decimal value,
        DomainEnum.RepType targetType,
        User? changedBy = null)
    {
        var sequence = GetNextSequence();

        blocks.Add(
            ExerciseBlock.New(
                Id,
                movementId,
                sequence,
                WorkTarget.New(value, targetType)
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

        block.ChangeSequence(newSequence);

        ResequenceBlocks();

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
        DomainEnum.RepType targetType,
        User? changedBy = null)
    {
        var block = blocks
            .FirstOrDefault(block => block.Id == exerciseBlockId);

        if (block is null)
        {
            return;
        }

        block.ChangeTarget(WorkTarget.New(value, targetType));

        Audit = Audit.MarkUpdated(changedBy);
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
        var orderedBlocks = blocks
            .OrderBy(block => block.Sequence)
            .ToList();

        for (var index = 0; index < orderedBlocks.Count; index++)
        {
            orderedBlocks[index].ChangeSequence(index + 1);
        }
    }
}