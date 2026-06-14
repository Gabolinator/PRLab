using PRLab.API.DTO.Description;
using PRLab.Domain.Model.Value.Update;

namespace PRLab.API.Mapper.UpdateMapper;

public static class DescriptionUpdateMapper
{
    public static DescriptionUpdate ToUpdate(DescriptionPutDTO payload)
    {
        ArgumentNullException.ThrowIfNull(payload);

        return new DescriptionUpdate
        {
            Content = payload.Content,
            Language = payload.Language,
        };
    }
}