using PRLab.API.DTO.User;
using PRLab.Domain.Model.Value.Update;

namespace PRLab.API.Mapper.UpdateMapper;

public static class UserUpdateMapper
{
    public static UserUpdate ToUpdate(UserPutDTO payload)
    {
       return new UserUpdate
       {
         Name = payload.Name,
         Role = payload.Role
       };
    }
    
    public static UserUpdate ToUpdate(CurrentUserPutDTO payload)
    {
        return new UserUpdate
        {
            Name = payload.Name,
            Role = null,
        };
    }
}