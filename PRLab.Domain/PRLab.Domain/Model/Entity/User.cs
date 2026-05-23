using PRLab.Utilities;
using PRLab.Value.Identifier;

namespace PRLab.Model.Entity;

public record User(UserId Id, string Name)
{
    public static class DefaultAdmin
    {
        public static readonly UserId Id = new(CoreUtilities.GuidGenerator.AdminId);
        public const string Name = "Admin";
    }
    
}