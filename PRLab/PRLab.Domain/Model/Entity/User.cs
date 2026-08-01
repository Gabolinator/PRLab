using PRLab.Domain.Model.Interface;
using PRLab.Domain.Model.Value;
using PRLab.Domain.Model.Value.Enum.System;
using PRLab.Domain.Model.Value.Identifier;
using PRLab.Domain.Model.Value.Update;
using PRLab.Domain.Utilities;

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

    public static User Admin(string? name = null, User? createdBy = null)
    {
        return PredefinedUsers.System.Create(name, createdBy?.Id);
    }
    
    public static User Existing(
        UserId id,
        string name,
        UserRole role,
        UserId? createdBy = null)
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
            AuditInfo.New(createdBy)
        );
    }
    
    public bool Update(
        UserUpdate update,
        User? changedBy = null)
    {
        ArgumentNullException.ThrowIfNull(update);

        var hasChanged = false;

        if (!string.IsNullOrWhiteSpace(update.Name))
        {
            hasChanged = TryRename(update.Name) || hasChanged;
        }

        if (update.Role.HasValue)
        {
            hasChanged = TryChangeRole(update.Role.Value) || hasChanged;
        }

        if (hasChanged)
        {
            MarkUpdated(changedBy);
        }

        return hasChanged;
    }

    private bool TryRename(string name)
    {
        var normalizedName =
            FormatingUtilities.NormalizeName(name);

        if (Name == normalizedName)
        {
            return false;
        }

        Name = normalizedName;

        return true;
    }

    private bool TryChangeRole(UserRole role)
    {
        if (!Enum.IsDefined(role))
        {
            throw new ArgumentOutOfRangeException(
                nameof(role),
                role,
                "Unsupported user role.");
        }

        if (Role == role)
        {
            return false;
        }

        Role = role;

        return true;
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

    public void MarkDeleted(User? deletedBy = null)
    {
        Audit = Audit.MarkDeleted(deletedBy);
    }
}