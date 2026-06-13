using PRLab.Domain.Model.Interface;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value;
using PRLab.Domain.Value.Enum.System;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Domain.Model.Entity;

public sealed record User : IAudited
{
    public UserId Id { get; init; }

    public string Name { get; private set; } = string.Empty;

    public UserRole Role { get; private set; }

    public AuditInfo Audit { get; private set; } = null!;

    private User()
    {
        // EF Core
    }

    private User(
        UserId id,
        string name,
        UserRole role,
        AuditInfo audit)
    {
        Id = id;
        Name = FormatingUtilities.NormalizeName(name);
        Role = role;
        Audit = audit;
    }

    public static User New(
        string name,
        UserRole role = UserRole.User,
        User? createdBy = null)
    {
        return new User(
            UserId.New(),
            name,
            role,
            AuditInfo.New(createdBy)
        );
    }

    public static User NewCoach(
        string name,
        User? createdBy = null)
    {
        return New(
            name,
            UserRole.Coach,
            createdBy
        );
    }

    public static User NewAdmin(
        string name,
        User? createdBy = null)
    {
        return New(
            name,
            UserRole.Admin,
            createdBy
        );
    }

    public static User Admin(string? name = null)
    {
        return new User(
            SystemUser.Id,
            !string.IsNullOrWhiteSpace(name) ? name : SystemUser.Name,
            UserRole.Admin,
            AuditInfo.New(null)
        );
    }
    
    public static User Existing(
        UserId id,
        string name,
        UserRole role)
    {
        if (id.Value == Guid.Empty)
        {
            throw new ArgumentException("User id cannot be empty.", nameof(id));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("User name cannot be empty.", nameof(name));
        }

        return new User(
            id,
            name,
            role,
            AuditInfo.New(null)
        );
    }
    
    public static class SystemUser
    {
        public static readonly UserId Id = UserId.FromGuid(
            Guid.Parse("00000000-0000-0000-0000-000000000001")
        );

        public const string Name = "Admin";
    }
    
    public static class DevelopmentUser
    {
        public static readonly UserId Id = UserId.FromGuid(
            Guid.Parse("00000000-0000-0000-0000-000000000002")
        );

        public const string Name = "Development User";

        public static User Create()
        {
            return Existing(
                Id,
                Name,
                UserRole.User
            );
        }
    }

    public void Rename(string name, User? changedBy = null)
    {
        Name = FormatingUtilities.NormalizeName(name);
        MarkUpdated(changedBy);
    }

    public void ChangeRole(
        UserRole role,
        User? changedBy = null)
    {
        if (Role == role)
        {
            return;
        }

        Role = role;
        MarkUpdated(changedBy);
    }

    void IAudited.MarkUpdated(User? changedBy)
    {
        MarkUpdated(changedBy);
    }

    void IAudited.MarkDeleted(User? deletedBy)
    {
        MarkDeleted(deletedBy);
    }

    private void MarkUpdated(User? changedBy = null)
    {
        Audit = Audit.MarkUpdated(changedBy);
    }

    private void MarkDeleted(User? deletedBy = null)
    {
        Audit = Audit.MarkDeleted(deletedBy);
    }
}