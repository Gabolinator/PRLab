using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB.Repositories;
using PRLab.Domain.Utilities;
using PRLab.Domain.Utilities.Interface;
using PRLab.Domain.Value.Identifier;
using PRLab.Infrastructure.DB.Context;

public class DescriptionRepository(PRLabPgDBContext db, IClock clock) : IDescriptionRepository
{
    public async Task<IReadOnlyList<Description>> GetAllAsync(CancellationToken ct)
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

        var exists = await ExistsByIdAsync(description.Id, ct) 
                     || await ExistsByContentAsync(description.GetContent(), ct);

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
        string? languageCode = null)
    {
        if (string.IsNullOrWhiteSpace(content))
        {
            throw new ArgumentException("Description content cannot be empty.", nameof(content));
        }

        var normalizedContent = FormatingUtilities.NormalizeNullableString(content);
        var normalizedLanguageCode = FormatingUtilities.NormalizeLanguageCodeOrDefault(languageCode, LocalizationHelper.DefaultLanguage);

        return await db.DescriptionTranslations
            .AsNoTracking()
            .AnyAsync(translation =>
                    translation.LanguageCode == normalizedLanguageCode &&
                    translation.Content == normalizedContent,
                ct);
    }
}