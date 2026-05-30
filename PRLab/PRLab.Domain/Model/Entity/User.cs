using PRLab.Domain.Model.Interface;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Domain.Model.Entity;

public sealed record User : IAudited
{
    public UserId Id { get; init; }

    public string Name { get; private set; } = string.Empty;

    public DomainEnum.UserRole Role { get; private set; }

    public AuditInfo Audit { get; private set; } = null!;

    private User()
    {
        // EF Core
    }

    private User(
        UserId id,
        string name,
        DomainEnum.UserRole role,
        AuditInfo audit)
    {
        Id = id;
        Name = FormatingUtilities.NormalizeName(name);
        Role = role;
        Audit = audit;
    }

    public static User New(
        string name,
        DomainEnum.UserRole role = DomainEnum.UserRole.User,
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
            DomainEnum.UserRole.Coach,
            createdBy
        );
    }

    public static User NewAdmin(
        string name,
        User? createdBy = null)
    {
        return New(
            name,
            DomainEnum.UserRole.Admin,
            createdBy
        );
    }

    public static User Admin()
    {
        return new User(
            DefaultAdmin.Id,
            DefaultAdmin.Name,
            DomainEnum.UserRole.Admin,
            AuditInfo.New(null)
        );
    }
    
    public static class DefaultAdmin
    {
        public static readonly UserId Id = new(CoreUtilities.GuidGenerator.Empty);

        public const string Name = "Admin";
    }

    public void Rename(string name, User? changedBy = null)
    {
        Name = FormatingUtilities.NormalizeName(name);
        MarkUpdated(changedBy);
    }

    public void ChangeRole(
        DomainEnum.UserRole role,
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