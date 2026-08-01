using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Repositories.Entity;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.Anatomy;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities;
using PRLab.Infrastructure.DB.Context;
using PRLab.Infrastructure.DB.Query;

namespace PRLab.Infrastructure.DB.Repositories.Entity;

public sealed class MuscleRepository(
    PRLabPgDBContext db) : IMuscleRepository
{
    public async Task<IReadOnlyList<Muscle>> ListAsync(
        CancellationToken ct)
    {
        return await BaseMuscleReadQuery()
            .OrderBy(muscle => muscle.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Muscle>> ListByAnyFunctionsAsync(
        IReadOnlyList<MuscleFunction> functions,
        IReadOnlyList<MuscleFunctionRole>? muscleFunctionRoles,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(functions);

        var distinctFunctions = functions
            .Distinct()
            .ToList();

        if (distinctFunctions.Count == 0)
        {
            return await BaseMuscleReadQuery()
                .OrderBy(muscle => muscle.Name)
                .ToListAsync(ct);
        }

        var query = BaseMuscleReadQuery();

        if (muscleFunctionRoles is null ||
            muscleFunctionRoles.Count == 0)
        {
            query = query.Where(
                muscle => muscle.Functions.Any(
                    functionAssignment =>
                        distinctFunctions.Contains(
                            functionAssignment.Function)));
        }
        else
        {
            var distinctRoles = muscleFunctionRoles
                .Distinct()
                .ToList();

            query = query.Where(
                muscle => muscle.Functions.Any(
                    functionAssignment =>
                        distinctFunctions.Contains(
                            functionAssignment.Function) &&
                        distinctRoles.Contains(
                            functionAssignment.Role)));
        }

        return await query
            .OrderBy(muscle => muscle.Name)
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Muscle>> ListByAllFunctionsAsync(
        IReadOnlyList<MuscleFunction> functions,
        IReadOnlyList<MuscleFunctionRole>? muscleFunctionRoles,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(functions);

        var distinctFunctions = functions
            .Distinct()
            .ToList();

        if (distinctFunctions.Count == 0)
        {
            return await BaseMuscleReadQuery()
                .OrderBy(muscle => muscle.Name)
                .ToListAsync(ct);
        }

        var query = BaseMuscleReadQuery();

        if (muscleFunctionRoles is null ||
            muscleFunctionRoles.Count == 0)
        {
            query = query.Where(
                muscle =>
                    muscle.Functions.Count(
                        functionAssignment =>
                            distinctFunctions.Contains(
                                functionAssignment.Function))
                    == distinctFunctions.Count);
        }
        else
        {
            var distinctRoles = muscleFunctionRoles
                .Distinct()
                .ToList();

            query = query.Where(
                muscle =>
                    muscle.Functions.Count(
                        functionAssignment =>
                            distinctFunctions.Contains(
                                functionAssignment.Function) &&
                            distinctRoles.Contains(
                                functionAssignment.Role))
                    == distinctFunctions.Count);
        }

        return await query
            .OrderBy(muscle => muscle.Name)
            .ToListAsync(ct);
    }
    
    public async Task<Muscle?> GetByIdAsync(
        MuscleId id,
        CancellationToken ct)
    {
        ValidateId(id);

        return await BaseMuscleReadQuery()
            .FirstOrDefaultAsync(
                muscle => muscle.Id == id,
                ct);
    }

    public async Task<Muscle?> GetTrackedByIdAsync(
        MuscleId id,
        CancellationToken ct)
    {
        ValidateId(id);

        return await BaseMuscleWriteQuery()
            .FirstOrDefaultAsync(
                muscle => muscle.Id == id,
                ct);
    }

    public async Task<Muscle> CreateAsync(
        Muscle muscle,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(muscle);

        ValidateId(muscle.Id);

        await db.Muscles.AddAsync(muscle, ct);
        await db.SaveChangesAsync(ct);

        /*
         * Reload as a complete read aggregate.
         *
         * This is especially useful when antagonists were created using only
         * their ids, because the AntagonistMuscle navigation may not yet be
         * populated on the originally created object.
         */
        return await GetRequiredForReadAsync(
            muscle.Id,
            ct);
    }

    public async Task<Muscle> UpdateAsync(
        Muscle muscle,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(muscle);

        ValidateId(muscle.Id);

        if (db.Entry(muscle).State == EntityState.Detached)
        {
            throw new InvalidOperationException(
                "Muscle must be loaded with GetByIdForUpdateAsync " +
                "before it can be updated.");
        }

        await db.SaveChangesAsync(ct);

        return muscle;
    }

    public async Task<bool> ExistsAsync(
        MuscleId id,
        CancellationToken ct)
    {
        ValidateId(id);

        return await BaseMuscleLookupQuery()
            .AnyAsync(
                muscle => muscle.Id == id,
                ct);
    }

    public async Task<bool> AllExistAsync(
        IReadOnlyCollection<MuscleId> ids,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(ids);

        if (ids.Count == 0)
        {
            return true;
        }

        ValidateIds(ids);

        var distinctIds = ids
            .Distinct()
            .ToList();

        var existingCount = await BaseMuscleLookupQuery()
            .CountAsync(
                muscle => distinctIds.Contains(muscle.Id),
                ct);

        return existingCount == distinctIds.Count;
    }

    public async Task<bool> NameExistsAsync(
        string name,
        MuscleId? excludedMuscleId,
        CancellationToken ct)
    {
        ValidateName(name);

        if (excludedMuscleId.HasValue)
        {
            ValidateId(excludedMuscleId.Value);
        }

        var nameKey = FormatingUtilities.NormalizeNameKey(name);

        return await BaseMuscleLookupQuery()
            .AnyAsync(
                muscle =>
                    muscle.NameKey == nameKey &&
                    (!excludedMuscleId.HasValue ||
                     muscle.Id != excludedMuscleId.Value),
                ct);
    }

    public async Task<Muscle> UpdateAntagonistsAsync(
        MuscleId id,
        IReadOnlyCollection<MuscleId> antagonistIds,
        CancellationToken ct)
    {
        ValidateId(id);

        ArgumentNullException.ThrowIfNull(antagonistIds);

        ValidateIds(antagonistIds);

        var distinctAntagonistIds = antagonistIds
            .Distinct()
            .ToList();

        if (distinctAntagonistIds.Contains(id))
        {
            throw new ArgumentException(
                "A muscle cannot be its own antagonist.",
                nameof(antagonistIds));
        }

        var muscle = await BaseMuscleWriteQuery()
            .FirstOrDefaultAsync(
                muscle => muscle.Id == id,
                ct);

        if (muscle is null)
        {
            throw new KeyNotFoundException(
                $"Muscle with id '{id}' was not found.");
        }

        var allAntagonistsExist = await AllExistAsync(
            distinctAntagonistIds,
            ct);

        if (!allAntagonistsExist)
        {
            throw new KeyNotFoundException(
                "One or more antagonist muscles were not found.");
        }

        var existingAntagonistIds = muscle.Antagonists
            .Select(antagonist => antagonist.AntagonistMuscleId)
            .ToHashSet();

        var requestedAntagonistIds = distinctAntagonistIds
            .ToHashSet();

        var antagonistIdsToRemove = existingAntagonistIds
            .Except(requestedAntagonistIds)
            .ToList();

        var antagonistIdsToAdd = requestedAntagonistIds
            .Except(existingAntagonistIds)
            .ToList();

        foreach (var antagonistId in antagonistIdsToRemove)
        {
            muscle.RemoveAntagonist(antagonistId);
        }

        foreach (var antagonistId in antagonistIdsToAdd)
        {
            muscle.AddAntagonist(antagonistId);
        }

        await db.SaveChangesAsync(ct);

        /*
         * Newly added antagonist join rows may not have their
         * AntagonistMuscle navigation populated. Return a refreshed,
         * fully loaded read model.
         */
        return await GetRequiredForReadAsync(
            id,
            ct);
    }

    private IQueryable<Muscle> BaseMuscleReadQuery()
    {
        return db.Muscles
            .ForFullRead();
    }

    private IQueryable<Muscle> BaseMuscleWriteQuery()
    {
        return db.Muscles
            .ForFullWrite();
    }

    private IQueryable<Muscle> BaseMuscleLookupQuery()
    {
        return db.Muscles
            .ActiveOnly()
            .AsNoTracking();
    }

    private async Task<Muscle> GetRequiredForReadAsync(
        MuscleId id,
        CancellationToken ct)
    {
        var muscle = await BaseMuscleReadQuery()
            .FirstOrDefaultAsync(
                muscle => muscle.Id == id,
                ct);

        return muscle
            ?? throw new KeyNotFoundException(
                $"Muscle with id '{id}' was not found.");
    }

    private static void ValidateId(MuscleId id)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Muscle id cannot be empty.",
                nameof(id));
        }
    }

    private static void ValidateIds(
        IReadOnlyCollection<MuscleId> ids)
        => DomainGuard.ValidRequiredIds(ids, nameof(ids));
    
    
    private static void ValidateId(EquipmentId id)
        => DomainGuard.ValidRequiredId(id, nameof(id));
    

    private static void ValidateName(string name)
        => DomainGuard.NotEmptyName(name, nameof(name));
}