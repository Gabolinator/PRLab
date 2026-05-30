using PRLab.API.Dtos.PutDto;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Update;

namespace PRLab.API.Mapper;

public static class EquipmentUpdateMapper
{
    public static EquipmentUpdate ToUpdate(EquipmentPutDTO payload, User? currentUser)
    {
        ArgumentNullException.ThrowIfNull(payload);

        return new EquipmentUpdate
        {
            Name = payload.Name,
            DescriptionUpdate = DescriptionUpdateMapper.ToUpdate(payload.Description),
            UpdatedBy = currentUser,
        };
    }
}