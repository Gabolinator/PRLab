using PRLab.Domain.Model.Interface;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Model.Value;
using PRLab.Domain.Model.Value.Enum.Anatomy;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Update;
using PRLab.Domain.Utilities;

namespace PRLab.Domain.Model.Entity;

public sealed record Muscle : IAudited, IDescribed
{
    public MuscleId Id { get; init; }

    public string Name { get; private set; } = string.Empty;
    
    public string NameKey { get; private set; } = string.Empty;

    public string? LatinName { get; private set; }

    public BodySection BodySection { get; private set; }
    
    private readonly List<MuscleFunctionAssignment> functions = [];

    public IReadOnlyCollection<MuscleFunctionAssignment> Functions => functions;

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
        string? latinName,
        BodySection bodySection,
        Description description,
        AuditInfo audit)
    {
        // muscles are always built-in so no visibility scope - there always Public
        
        DomainGuard.NotEmptyId(
            id.Value,
            nameof(id));

        DomainGuard.NotEmptyName(
            name,
            nameof(name));
        
        ArgumentNullException.ThrowIfNull(description);
        ArgumentNullException.ThrowIfNull(audit);
        
        Id = id;
        SetName(name);
        LatinName = NormalizeLatinName(latinName);
        BodySection = bodySection;
        Description = description;
        Audit = audit;
    }

    public static Muscle New(
        string name,
        string? latinName,
        BodySection bodySection,
        Description description,
        User? createdBy = null)
    {
        ArgumentNullException.ThrowIfNull(description);

        return new Muscle(
            MuscleId.New(),
            name,
            latinName,
            bodySection,
            description,
            AuditInfo.New(createdBy)
        );
    }

    public static Muscle New(
        string name,
        string? latinName,
        BodySection bodySection,
        string? description,
        User? createdBy = null)
    {
        return new Muscle(
            MuscleId.New(),
            name,
            latinName,
            bodySection,
            Description.New(description),
            AuditInfo.New(createdBy)
        );
    }

    public static Muscle NewWithId(
        MuscleId id,
        string name,
        string? latinName,
        BodySection bodySection,
        Description description,
        User? createdBy = null)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Muscle id cannot be empty.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(description);

        return new Muscle(
            id,
            name,
            latinName,
            bodySection,
            description,
            AuditInfo.New(createdBy)
        );
    }
    
    public bool Update(MuscleUpdate update)
    {
        ArgumentNullException.ThrowIfNull(update);

        var hasChanged = false;

        if (!string.IsNullOrWhiteSpace(update.Name))
        {
            SetName(update.Name);
            hasChanged = true;
        }

        if (update.LatinNameWasProvided)
        {
            LatinName = NormalizeLatinName(update.LatinName);
            hasChanged = true;
        }

        if (update.BodySection.HasValue)
        {
            BodySection = update.BodySection.Value;
            hasChanged = true;
        }

        if (update.DescriptionUpdate is not null)
        {
            Description = Description.ChangeContent(
                update.DescriptionUpdate.Content,
                update.DescriptionUpdate.Language
            );

            hasChanged = true;
        }

        if (hasChanged)
        {
            MarkUpdated(update.UpdatedBy);
        }

        return hasChanged;
    }
    
    public bool AddFunction(
        MuscleFunction function,
        MuscleFunctionRole role,
        User? changedBy = null)
    {
        if (!Enum.IsDefined(function))
        {
            throw new ArgumentOutOfRangeException(nameof(function));
        }

        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(nameof(role));
        }

        var existingAssignment = functions.FirstOrDefault(
            assignment => assignment.Function == function);

        if (existingAssignment is not null)
        {
            return false;
        }

        functions.Add(
            MuscleFunctionAssignment.New(
                Id,
                function,
                role));

        MarkUpdated(changedBy);

        return true;
    }
    
    public bool ReplaceFunctions(
    IReadOnlyCollection<MuscleFunctionDefinition> requestedFunctions,
    User? changedBy = null)
{
    ArgumentNullException.ThrowIfNull(requestedFunctions);

    foreach (var requestedFunction in requestedFunctions)
    {
        if (!Enum.IsDefined(requestedFunction.Function))
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedFunctions),
                requestedFunction.Function,
                "Unsupported muscle function.");
        }

        if (!Enum.IsDefined(requestedFunction.Role))
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestedFunctions),
                requestedFunction.Role,
                "Unsupported muscle function role.");
        }
    }

    var duplicateFunctions = requestedFunctions
        .GroupBy(requestedFunction => requestedFunction.Function)
        .Where(group => group.Count() > 1)
        .Select(group => group.Key)
        .ToList();

    if (duplicateFunctions.Count > 0)
    {
        throw new ArgumentException(
            $"Duplicate muscle functions are not allowed: " +
            $"{string.Join(", ", duplicateFunctions)}.",
            nameof(requestedFunctions));
    }

    var requestedFunctionsByType = requestedFunctions
        .ToDictionary(
            requestedFunction => requestedFunction.Function,
            requestedFunction => requestedFunction.Role);

    var hasChanged = false;

    foreach (var existingFunction in functions.ToList())
    {
        if (!requestedFunctionsByType.TryGetValue(
                existingFunction.Function,
                out var requestedRole))
        {
            functions.Remove(existingFunction);
            hasChanged = true;

            continue;
        }

        hasChanged =
            existingFunction.ChangeRole(requestedRole) ||
            hasChanged;
    }

    var existingFunctionTypes = functions
        .Select(functionAssignment => functionAssignment.Function)
        .ToHashSet();

    foreach (var requestedFunction in requestedFunctions)
    {
        if (existingFunctionTypes.Contains(requestedFunction.Function))
        {
            continue;
        }

        functions.Add(
            MuscleFunctionAssignment.New(
                Id,
                requestedFunction.Function,
                requestedFunction.Role));

        hasChanged = true;
    }

    if (hasChanged)
    {
        MarkUpdated(changedBy);
    }

    return hasChanged;
}
    
    public bool HasAnyFunctions(
        IReadOnlyCollection<MuscleFunction> muscleFunctions,
        params MuscleFunctionRole[] functionRoles)
    {
        ArgumentNullException.ThrowIfNull(muscleFunctions);
        ArgumentNullException.ThrowIfNull(functionRoles);

        if (muscleFunctions.Count == 0 ||
            Functions.Count == 0)
        {
            return false;
        }

        var requestedFunctions = muscleFunctions.ToHashSet();

        if (functionRoles.Length == 0)
        {
            return Functions.Any(
                functionAssignment =>
                    requestedFunctions.Contains(
                        functionAssignment.Function));
        }

        var acceptedRoles = functionRoles.ToHashSet();

        return Functions.Any(
            functionAssignment =>
                requestedFunctions.Contains(
                    functionAssignment.Function) &&
                acceptedRoles.Contains(
                    functionAssignment.Role));
    }
    
    public bool HasAllFunctions(
        IReadOnlyCollection<MuscleFunction> muscleFunctions,
        params MuscleFunctionRole[] functionRoles)
    {
        ArgumentNullException.ThrowIfNull(muscleFunctions);
        ArgumentNullException.ThrowIfNull(functionRoles);

        var requestedFunctions = muscleFunctions
            .ToHashSet();

        if (requestedFunctions.Count == 0)
        {
            return true;
        }

        if (Functions.Count == 0)
        {
            return false;
        }

        if (functionRoles.Length == 0)
        {
            var availableFunctions = Functions
                .Select(functionAssignment =>
                    functionAssignment.Function)
                .ToHashSet();

            return requestedFunctions.IsSubsetOf(
                availableFunctions);
        }

        var acceptedRoles = functionRoles
            .ToHashSet();

        var availableFunctionsForRoles = Functions
            .Where(functionAssignment =>
                acceptedRoles.Contains(functionAssignment.Role))
            .Select(functionAssignment =>
                functionAssignment.Function)
            .ToHashSet();

        return requestedFunctions.IsSubsetOf(
            availableFunctionsForRoles);
    }
    
    private void SetName(string name)
    {
        Name = FormatingUtilities.NormalizeName(name);
        NameKey = FormatingUtilities.NormalizeNameKey(name);
    }

    public void ChangeBodySection(
        BodySection bodySection,
        User? changedBy = null)
    {
        BodySection = bodySection;
        MarkUpdated(changedBy);
    }

    public void Rename(
        string name,
        User? changedBy = null)
    {
        SetName(name);
        MarkUpdated(changedBy);
    }

    public void ChangeLatinName(
        string? latinName,
        User? changedBy = null)
    {
        LatinName = NormalizeLatinName(latinName);
        MarkUpdated(changedBy);
    }

    public void AddAntagonist(
        MuscleId antagonistMuscleId,
        User? changedBy = null)
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

    public void RemoveAntagonist(
        MuscleId antagonistMuscleId,
        User? changedBy = null)
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

    void IAudited.MarkUpdated(User? changedBy)
    {
        MarkUpdated(changedBy);
    }

    
    public void Delete(User? deletedBy = null)
    {
        if (Audit.IsDeleted)
        {
            return;
        }

        MarkDeleted(deletedBy);
    }
    
    public void Restore(User? restoredBy = null)
    {
        if (!Audit.IsDeleted)
        {
            return;
        }

        MarkRestored(restoredBy);
    }

    private void MarkRestored(User? restoredBy)
    {
        Audit = Audit.MarkRestored(restoredBy);
    }

    void IAudited.MarkDeleted(User? deletedBy)
    {
        MarkDeleted(deletedBy);
    }

    private static string? NormalizeLatinName(string? latinName)
    {
        if (string.IsNullOrWhiteSpace(latinName))
        {
            return null;
        }

        return latinName.Trim();
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