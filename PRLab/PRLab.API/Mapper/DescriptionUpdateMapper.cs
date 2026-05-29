using PRLab.API.Dtos.PutDto;
using PRLab.Domain.Value.Update;

namespace PRLab.API.Mapper;

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