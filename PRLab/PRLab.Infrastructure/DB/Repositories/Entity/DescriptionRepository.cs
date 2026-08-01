using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Repositories.Entity;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities;
using PRLab.Infrastructure.DB.Context;
using PRLab.Infrastructure.DB.Query;

namespace PRLab.Infrastructure.DB.Repositories.Entity;

public sealed class DescriptionRepository(
    PRLabPgDBContext db) : IDescriptionRepository
{
    public async Task<IReadOnlyCollection<Description>> ListAsync(
        CancellationToken ct)
    {
        return await BaseDescriptionReadQuery()
            .OrderBy(description => description.Id)
            .ToListAsync(ct);
    }

    public async Task<Description?> GetByIdAsync(
        DescriptionId id,
        CancellationToken ct)
    {
        ValidateId(id);

        return await BaseDescriptionReadQuery()
            .FirstOrDefaultAsync(
                description => description.Id == id,
                ct);
    }

    public async Task<Description?> GetByIdForUpdateAsync(
        DescriptionId id,
        CancellationToken ct)
    {
        ValidateId(id);

        return await BaseDescriptionWriteQuery()
            .FirstOrDefaultAsync(
                description => description.Id == id,
                ct);
    }

    public async Task<int> GetCountAsync(
        CancellationToken ct)
    {
        return await BaseDescriptionLookupQuery()
            .CountAsync(ct);
    }

    public async Task<Description> CreateAsync(
        Description description,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(description);

        ValidateId(description.Id);

        if (await ExistsByIdAsync(description.Id, ct))
        {
            throw new ArgumentException(
                $"Description with id '{description.Id}' already exists.",
                nameof(description));
        }

        await db.Description.AddAsync(description, ct);
        await db.SaveChangesAsync(ct);

        return description;
    }

    public async Task<Description> UpdateAsync(
        Description description,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(description);

        ValidateId(description.Id);

        if (db.Entry(description).State == EntityState.Detached)
        {
            throw new InvalidOperationException(
                "Description must be loaded with GetByIdForUpdateAsync " +
                "before it can be updated.");
        }

        await db.SaveChangesAsync(ct);

        return description;
    }

    public async Task<Description> GetOrCreateAsync(
        Description description,
        CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(description);

        ValidateId(description.Id);

        var existingDescription = await BaseDescriptionWriteQuery()
            .FirstOrDefaultAsync(
                existing => existing.Id == description.Id,
                ct);

        if (existingDescription is not null)
        {
            return existingDescription;
        }

        await db.Description.AddAsync(description, ct);
        await db.SaveChangesAsync(ct);

        return description;
    }

    public async Task<bool> ExistsByIdAsync(
        DescriptionId id,
        CancellationToken ct)
    {
        ValidateId(id);

        return await BaseDescriptionLookupQuery()
            .AnyAsync(
                description => description.Id == id,
                ct);
    }

    public async Task<bool> ExistsByContentAsync(
        string? content,
        CancellationToken ct,
        LocalizationHelper.Language? languageCode = null)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException(
                "Description content cannot be empty.",
                nameof(content));
        }

        var normalizedContent =
            FormatingUtilities.NormalizeDescriptionContent(content);

        var normalizedLanguageCode =
            LocalizationHelper.ToLanguageCodeOrDefault(languageCode);

        return await db.DescriptionTranslations
            .AsNoTracking()
            .AnyAsync(
                translation =>
                    translation.LanguageCode == normalizedLanguageCode &&
                    translation.Content == normalizedContent,
                ct);
    }

    private IQueryable<Description> BaseDescriptionReadQuery()
    {
        return db.Description
            .ForFullRead();
    }

    private IQueryable<Description> BaseDescriptionWriteQuery()
    {
        return db.Description
            .ForFullWrite();
    }

    private IQueryable<Description> BaseDescriptionLookupQuery()
    {
        return db.Description
            .AsNoTracking();
    }

    private static void ValidateId(DescriptionId id)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException(
                "Description id cannot be empty.",
                nameof(id));
        }
    }
}