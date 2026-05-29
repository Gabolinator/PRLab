using PRLab.Domain.Model.Entity;
using PRLab.Domain.Utilities;

namespace PRLab.Domain.Model.Interface;

public interface IDescribed
{
    Description Description { get; }

    void ChangeDescription(
        string? content,
        LocalizationHelper.Language? languageCode,
        User? changedBy = null);

    void RemoveDescription(
        LocalizationHelper.Language? languageCode,
        User? changedBy = null);
}