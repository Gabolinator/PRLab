using PRLab.Domain.Model.Entity;

namespace PRLab.Domain.Model.Interface;

public interface IDescribed
{
    Description Description { get; }

    void ChangeDescription(
        string? content,
        string languageCode = "en",
        User? changedBy = null);

    void RemoveDescription(
        string languageCode = "en",
        User? changedBy = null);
}