using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Repositories.Entity;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value.Identifier;
using PRLab.Infrastructure.DB.Context;

namespace PRLab.Infrastructure.DB.Repositories.Entity;

public sealed class MuscleRepository(PRLabPgDBContext db) : IMuscleRepository
{
    public async Task<IReadOnlyList<Muscle>> ListAsync(CancellationToken ct)
    {
        return await db.Muscles
            .AsNoTracking()
            .Include(muscle => muscle.Description)
            .ThenInclude(description => description.Translations)
            .Include(muscle => muscle.Antagonists)
            .ThenInclude(antagonist => antagonist.AntagonistMuscle)
            .Where(muscle => !muscle.Audit.IsDeleted)
            .ToListAsync(ct);
    }

    public async Task<Muscle?> GetByIdAsync(MuscleId id, CancellationToken ct)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Muscle id cannot be empty.", nameof(id));
        }

        return await db.Muscles
            .Include(muscle => muscle.Description)
                .ThenInclude(description => description.Translations)
            .Include(muscle => muscle.Antagonists)
            .ThenInclude(antagonist => antagonist.AntagonistMuscle)
            .FirstOrDefaultAsync(
                muscle => muscle.Id == id && !muscle.Audit.IsDeleted,
                ct);
    }

    public async Task<Muscle> CreateAsync(Muscle muscle, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(muscle);

        await db.Muscles.AddAsync(muscle, ct);
        await db.SaveChangesAsync(ct);

        return muscle;
    }

    public async Task<Muscle> UpdateAsync(Muscle muscle, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(muscle);

        if (muscle.Id.Value == Guid.Empty)
        {
            throw new ArgumentException("Muscle id cannot be empty.", nameof(muscle));
        }

        db.Muscles.Update(muscle);
        await db.SaveChangesAsync(ct);

        return muscle;
    }

    public async Task<bool> ExistsAsync(MuscleId id, CancellationToken ct)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Muscle id cannot be empty.", nameof(id));
        }

        return await db.Muscles
            .AsNoTracking()
            .AnyAsync(
                muscle => muscle.Id == id && !muscle.Audit.IsDeleted,
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

        if (ids.Any(id => id.Value == Guid.Empty))
        {
            throw new ArgumentException("Muscle ids cannot contain empty values.", nameof(ids));
        }

        var distinctIds = ids
            .Distinct()
            .ToList();

        var existingCount = await db.Muscles
            .AsNoTracking()
            .Where(muscle => !muscle.Audit.IsDeleted)
            .CountAsync(muscle => distinctIds.Contains(muscle.Id), ct);

        return existingCount == distinctIds.Count;
    }

    public async Task<Muscle> UpdateAntagonistsAsync(
        MuscleId id,
        IReadOnlyCollection<MuscleId> antagonistIds,
        CancellationToken ct)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Muscle id cannot be empty.", nameof(id));
        }

        ArgumentNullException.ThrowIfNull(antagonistIds);

        if (antagonistIds.Any(antagonistId => antagonistId.Value == Guid.Empty))
        {
            throw new ArgumentException("Antagonist ids cannot contain empty values.", nameof(antagonistIds));
        }

        if (antagonistIds.Contains(id))
        {
            throw new ArgumentException("A muscle cannot be its own antagonist.", nameof(antagonistIds));
        }

        var distinctAntagonistIds = antagonistIds
            .Distinct()
            .ToList();

        var muscle = await db.Muscles
            .Include(muscle => muscle.Description)
                .ThenInclude(description => description.Translations)
            .Include(muscle => muscle.Antagonists)
            .FirstOrDefaultAsync(
                muscle => muscle.Id == id && !muscle.Audit.IsDeleted,
                ct);

        if (muscle is null)
        {
            throw new InvalidOperationException("Muscle was not found.");
        }

        var allAntagonistsExist = await AllExistAsync(distinctAntagonistIds, ct);

        if (!allAntagonistsExist)
        {
            throw new InvalidOperationException("One or more antagonist muscles were not found.");
        }

        var existingAntagonistIds = muscle.Antagonists
            .Select(antagonist => antagonist.AntagonistMuscleId)
            .ToHashSet();

        var requestedAntagonistIds = distinctAntagonistIds
            .ToHashSet();

        var antagonistIdsToRemove = existingAntagonistIds
            .Where(existingAntagonistId => !requestedAntagonistIds.Contains(existingAntagonistId))
            .ToList();

        var antagonistIdsToAdd = requestedAntagonistIds
            .Where(requestedAntagonistId => !existingAntagonistIds.Contains(requestedAntagonistId))
            .ToList();

        foreach (var antagonistIdToRemove in antagonistIdsToRemove)
        {
            muscle.RemoveAntagonist(antagonistIdToRemove);
        }

        foreach (var antagonistIdToAdd in antagonistIdsToAdd)
        {
            muscle.AddAntagonist(antagonistIdToAdd);
        }

        await db.SaveChangesAsync(ct);

        return muscle;
    }
    
    public async Task<bool> NameExistsAsync(
        string name,
        MuscleId? excludedMuscleId,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Muscle name cannot be empty.", nameof(name));
        }

        var nameKey = FormatingUtilities.NormalizeNameKey(name);

        return await db.Muscles
            .AsNoTracking()
            .AnyAsync(
                muscle =>
                    muscle.NameKey == nameKey &&
                    !muscle.Audit.IsDeleted &&
                    (!excludedMuscleId.HasValue || muscle.Id != excludedMuscleId.Value),
                ct);
    }
    
}