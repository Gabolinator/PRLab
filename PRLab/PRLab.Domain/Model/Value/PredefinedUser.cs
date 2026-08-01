using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Domain.Model.Value;

public static class PredefinedUsers
{
    public static class System
    {
        public static readonly UserId Id = UserId.FromGuid(
            new Guid("00000000-0000-0000-0000-000000000001"));

        public const string Name = "Admin";

        public static User Create(string? name = null, UserId? createdBy = null)
        {
            return User.Existing(
                Id,
                !string.IsNullOrWhiteSpace(name) ? name: Name,
                UserRole.Admin,
                createdBy);
        }
    }

    public static class Development
    {
        public static readonly UserId Id = UserId.FromGuid(
            new Guid("00000000-0000-0000-0000-000000000002"));

        public const string Name = "Development User";

        public static User Create(
            Guid? guid = null,
            string? name = null,
            UserRole role = UserRole.User)
        {
            return User.Existing(
                guid.HasValue
                    ? UserId.FromGuid(guid.Value)
                    : Id,
                string.IsNullOrWhiteSpace(name)
                    ? Name
                    : name,
                role);
        }
    }
}
