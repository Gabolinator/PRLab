using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Repositories.Entity;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Utilities;
using PRLab.Domain.Utilities.Interface;
using PRLab.Infrastructure.DB.Context;

namespace PRLab.Infrastructure.DB.Repositories.Entity;

public class DescriptionRepository(PRLabPgDBContext db, IClock clock) : IDescriptionRepository
{
    public async Task<IReadOnlyCollection<Description>> ListAsync(CancellationToken ct)
    {
        return await db.Description
            .AsNoTracking()
            .Include(description => description.Translations)
            .ToListAsync(ct);
    }

    public async Task<Description?> GetByIdAsync(
        DescriptionId id,
        CancellationToken ct)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Description id cannot be empty.", nameof(id));
        }

        return await db.Description
            .AsNoTracking()
            .Include(description => description.Translations)
            .FirstOrDefaultAsync(description => description.Id == id, ct);
    }

    public async Task<int> GetCountAsync(CancellationToken ct)
    {
        return await db.Description
            .AsNoTracking()
            .CountAsync(ct);
    }

    public async Task<Description> CreateAsync(Description description, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(description);

        var exists = await ExistsByIdAsync(description.Id, ct);

        if (exists)
        {
            throw new ArgumentException(
                $"Description with id '{description.Id}' already exist found.");
        }
        
        await db.Description.AddAsync(description, ct);
        await db.SaveChangesAsync(ct);

        return description;
    }

    public async Task<Description> UpdateAsync(Description description, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(description);

        if (description.Id.Value == Guid.Empty)
        {
            throw new ArgumentException("Description id cannot be empty.", nameof(description));
        }

        var exists = await ExistsByIdAsync(description.Id, ct);

        if (!exists)
        {
            throw new KeyNotFoundException(
                $"Description with id '{description.Id}' was not found.");
        }

        db.Description.Update(description);
        await db.SaveChangesAsync(ct);

        return description;
    }

    public async Task<Description> GetOrCreateAsync(Description description, CancellationToken ct)
    {
        ArgumentNullException.ThrowIfNull(description);
        
        var existingDescription = await GetByIdAsync(description.Id,ct);

        if (existingDescription is not null)
        {
            return existingDescription;
        }

        await db.Description.AddAsync(description, ct);
        await db.SaveChangesAsync(ct);

        return description;
    }

    public async Task<bool> ExistsByIdAsync(DescriptionId id, CancellationToken ct)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("Description id cannot be empty.", nameof(id));
        }

        return await db.Description
            .AsNoTracking()
            .AnyAsync(description => description.Id == id.Value, ct);
    }
    
    public async Task<bool> ExistsByContentAsync(
        string? content,
        CancellationToken ct,
        LocalizationHelper.Language? languageCode = null)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Description content cannot be empty.", nameof(content));
        }

        var normalizedContent = FormatingUtilities.NormalizeDescriptionContent(content);
        var normalizedLanguageCode = LocalizationHelper.ToLanguageCodeOrDefault(languageCode);

        return await db.DescriptionTranslations
            .AsNoTracking()
            .AnyAsync(translation =>
                    translation.LanguageCode == normalizedLanguageCode &&
                    translation.Content == normalizedContent,
                ct);
    }
}