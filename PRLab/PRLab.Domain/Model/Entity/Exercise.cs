using PRLab.Domain.Model.Interface;
using PRLab.Domain.Model.Value;
using PRLab.Domain.Model.Value.Enum.Prescription;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Ownership;
using PRLab.Domain.Model.Value.Prescription;
using PRLab.Domain.Model.Value.Update;
using PRLab.Domain.Utilities;

namespace PRLab.Domain.Model.Entity;

public sealed record Exercise : IAudited, IDescribed, IOwnedData
{
    public ExerciseId Id { get; init; }

    public string Name { get; private set; } = string.Empty;
    
    public string NameKey { get; private set; } = string.Empty;

    public Description Description { get; private set; } = null!;
    
    public OwnershipInfo Ownership { get; private set; } = null!;

    public AuditInfo Audit { get; private set; } = null!;

    private readonly List<ExerciseSteps> steps = [];

    public IReadOnlyCollection<ExerciseSteps> Steps => steps
        .OrderBy(Step => Step.Sequence)
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

    public static Exercise NewBuiltInWithId(
        ExerciseId id,
        string name,
        Description description,
        User? createdBy = null)
    {
        return new Exercise(
            id,
            name,
            description,
            AuditInfo.New(createdBy),
            OwnershipInfo.BuiltIn()
        );
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
        WorkTargetType targetType,
        User owner,
        LoadTarget? loadTarget = null,
        RestTarget? restBetweenReps = null,
        RestTarget? transitionAfterStep = null,
        RepExecutionDetails? executionDetails = null)
    {
        ArgumentNullException.ThrowIfNull(movement);
        ArgumentNullException.ThrowIfNull(owner);

        var exercise = NewUserCreated(
            name: movement.Name,
            description: movement.Description.Copy(),
            owner: owner);

        exercise.AddStep(
            movementId: movement.Id,
            target: WorkTarget.New(value, targetType),
            loadTarget: loadTarget,
            restBetweenReps: restBetweenReps,
            transitionAfterStep: transitionAfterStep,
            executionDetails: executionDetails,
            changedBy: owner);

        return exercise;
    }

    public static Exercise FromMovementBuiltIn(Movement movement)
    {
        ArgumentNullException.ThrowIfNull(movement);

        var exercise = NewBuiltIn(
            movement.Name,
            movement.Description.Copy());

        var targetType = movement.DefaultWorkTargetType;

        exercise.AddStep(
            movementId: movement.Id,
            target: WorkTarget.FromDefaultWorkType(targetType),
            loadTarget: LoadTarget.FromMovement(movement));

        return exercise;
    }
    
    
    public static Exercise FromMovementBuiltIn(
        Movement movement,
        decimal value,
        WorkTargetType targetType,
        User? createdBy = null,
        LoadTarget? loadTarget = null,
        RestTarget? restBetweenReps = null,
        RestTarget? transitionAfterStep = null,
        RepExecutionDetails? executionDetails = null)
    {
        ArgumentNullException.ThrowIfNull(movement);

        var exercise = NewBuiltIn(
            name: movement.Name,
            description: movement.Description.Copy(),
            createdBy: createdBy);

        exercise.AddStep(
            movementId: movement.Id,
            target: WorkTarget.New(value, targetType),
            loadTarget: loadTarget,
            restBetweenReps: restBetweenReps,
            transitionAfterStep: transitionAfterStep,
            executionDetails: executionDetails,
            changedBy: createdBy);

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

    public bool Update(ExerciseUpdate update)
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

        if (update.Steps is not null)
        {
            ReplaceSteps(update.Steps);
            hasChanged = true;
        }

        if (hasChanged)
        {
            MarkUpdated(update.UpdatedBy);
        }

        return hasChanged;
    }

    private void ReplaceSteps(IReadOnlyCollection<ExerciseStepUpdate> updatedSteps)
    {
        steps.Clear();

        foreach (var updatedStep in updatedSteps.OrderBy(Step => Step.Sequence))
        {
            steps.Add(
                ExerciseSteps.New(
                    exerciseId: Id,
                    movementId: updatedStep.MovementId,
                    sequence: updatedStep.Sequence,
                    target: updatedStep.Target,
                    loadTarget: updatedStep.LoadTarget,
                    restBetweenReps: updatedStep.RestBetweenReps,
                    transitionAfterStep: updatedStep.TransitionAfterStep,
                    executionDetails: updatedStep.ExecutionDetails
                )
            );
        }

        ResequenceSteps();
    }

    public void AddStep(
        ExerciseSteps steps,
        User? changedBy = null,
        int? atSequence = null)
    {
        ArgumentNullException.ThrowIfNull(steps);

        var targetSequence = NormalizeInsertSequence(atSequence ?? steps.Sequence);

        ShiftStepsFromSequence(targetSequence);

        steps.ChangeSequence(targetSequence);

        this.steps.Add(steps);

        MarkUpdated(changedBy);
    }

    public void AddStep(
        MovementId movementId,
        WorkTarget target,
        LoadTarget? loadTarget = null,
        RestTarget? restBetweenReps = null,
        RestTarget? transitionAfterStep = null,
        RepExecutionDetails? executionDetails = null,
        User? changedBy = null,
        int? atSequence = null)
    {
        ArgumentNullException.ThrowIfNull(target);

        var Step = ExerciseSteps.New(
            exerciseId: Id,
            movementId: movementId,
            sequence: GetNextSequence(),
            target: target,
            loadTarget: loadTarget,
            restBetweenReps: restBetweenReps,
            transitionAfterStep: transitionAfterStep,
            executionDetails: executionDetails
        );

        AddStep(
            Step,
            changedBy,
            atSequence);
    }

    public void AddStep(
        MovementId movementId,
        decimal value,
        WorkTargetType targetType,
        LoadTarget? loadTarget = null,
        RestTarget? restBetweenReps = null,
        RestTarget? transitionAfterStep = null,
        RepExecutionDetails? executionDetails = null,
        User? changedBy = null,
        int? atSequence = null)
    {
        AddStep(
            movementId: movementId,
            target: WorkTarget.New(value, targetType),
            loadTarget: loadTarget,
            restBetweenReps: restBetweenReps,
            transitionAfterStep: transitionAfterStep,
            executionDetails: executionDetails,
            changedBy: changedBy,
            atSequence: atSequence);
    }
    
    public void RemoveStep(ExerciseStepsId exerciseStepsId, User? changedBy = null)
    {
        var Step = steps
            .FirstOrDefault(existingStep => existingStep.Id == exerciseStepsId);

        if (Step is null)
        {
            return;
        }

        steps.Remove(Step);
        ResequenceSteps();
        MarkUpdated(changedBy);
    }

    public void MoveStep(
        ExerciseStepsId exerciseStepsId,
        int newSequence,
        User? changedBy = null)
    {
        if (newSequence < 1)
        {
            throw new ArgumentException("Sequence must be greater than zero.");
        }

        var Step = steps
            .FirstOrDefault(Step => Step.Id == exerciseStepsId);

        if (Step is null)
        {
            return;
        }

        steps.Remove(Step);

        var targetIndex = Math.Min(
            newSequence - 1,
            steps.Count
        );

        steps.Insert(
            targetIndex,
            Step
        );

        ResequenceSteps();

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

    public void ChangeStepTarget(
        ExerciseStepsId exerciseStepsId,
        decimal value,
        WorkTargetType targetType,
        User? changedBy = null)
    {
        var Step = GetStepOrDefault(exerciseStepsId);

        if (Step is null)
        {
            return;
        }

        Step.ChangeTarget(WorkTarget.New(value, targetType));

        MarkUpdated(changedBy);
    }

    public void ChangeStepLoadTarget(
        ExerciseStepsId exerciseStepsId,
        LoadTarget loadTarget,
        User? changedBy = null)
    {
        var Step = GetStepOrDefault(exerciseStepsId);

        if (Step is null)
        {
            return;
        }

        Step.ChangeLoadTarget(loadTarget);

        MarkUpdated(changedBy);
    }

    public void RemoveStepLoadTarget(
        ExerciseStepsId exerciseStepsId,
        User? changedBy = null)
    {
        var Step = GetStepOrDefault(exerciseStepsId);

        if (Step is null)
        {
            return;
        }

        Step.RemoveLoadTarget();

        MarkUpdated(changedBy);
    }

    public void ChangeStepRestBetweenReps(
        ExerciseStepsId exerciseStepsId,
        RestTarget restBetweenReps,
        User? changedBy = null)
    {
        var Step = GetStepOrDefault(exerciseStepsId);

        if (Step is null)
        {
            return;
        }

        Step.ChangeRestBetweenReps(restBetweenReps);

        MarkUpdated(changedBy);
    }

    public void RemoveStepRestBetweenReps(
        ExerciseStepsId exerciseStepsId,
        User? changedBy = null)
    {
        var Step = GetStepOrDefault(exerciseStepsId);

        if (Step is null)
        {
            return;
        }

        Step.RemoveRestBetweenReps();

        MarkUpdated(changedBy);
    }

    public void ChangeStepTransitionAfterStep(
        ExerciseStepsId exerciseStepsId,
        RestTarget transitionAfterStep,
        User? changedBy = null)
    {
        var Step = GetStepOrDefault(exerciseStepsId);

        if (Step is null)
        {
            return;
        }

        Step.ChangeTransitionAfterStep(transitionAfterStep);

        MarkUpdated(changedBy);
    }

    public void RemoveStepTransitionAfterStep(
        ExerciseStepsId exerciseStepsId,
        User? changedBy = null)
    {
        var Step = GetStepOrDefault(exerciseStepsId);

        if (Step is null)
        {
            return;
        }

        Step.RemoveTransitionAfterStep();

        MarkUpdated(changedBy);
    }

    public void ChangeStepExecutionDetails(
        ExerciseStepsId exerciseStepsId,
        RepExecutionDetails executionDetails,
        User? changedBy = null)
    {
        var Step = GetStepOrDefault(exerciseStepsId);

        if (Step is null)
        {
            return;
        }

        Step.ChangeExecutionDetails(executionDetails);

        MarkUpdated(changedBy);
    }

    public void RemoveStepExecutionDetails(
        ExerciseStepsId exerciseStepsId,
        User? changedBy = null)
    {
        var Step = GetStepOrDefault(exerciseStepsId);

        if (Step is null)
        {
            return;
        }

        Step.RemoveExecutionDetails();

        MarkUpdated(changedBy);
    }

    private ExerciseSteps? GetStepOrDefault(ExerciseStepsId exerciseStepsId)
    {
        return steps
            .FirstOrDefault(Step => Step.Id == exerciseStepsId);
    }

    private int GetNextSequence()
    {
        if (steps.Count == 0)
        {
            return 1;
        }

        return steps.Max(Step => Step.Sequence) + 1;
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
                "Step sequence must be greater than zero.",
                nameof(atSequence));
        }

        if (atSequence > steps.Count)
        {
            return nextSequence;
        }

        return atSequence.Value;
    }

    private void ShiftStepsFromSequence(int sequence)
    {
        foreach (var Step in steps
                     .Where(existingStep => existingStep.Sequence >= sequence)
                     .OrderByDescending(existingStep => existingStep.Sequence))
        {
            Step.ChangeSequence(Step.Sequence + 1);
        }
    }

    private void ResequenceSteps()
    {
        var orderedSteps = steps
            .OrderBy(Step => Step.Sequence)
            .ToList();

        for (var index = 0; index < orderedSteps.Count; index++)
        {
            orderedSteps[index].ChangeSequence(index + 1);
        }
    }
}