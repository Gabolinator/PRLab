using FluentAssertions;
using PRLab.Domain;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Interface;
using PRLab.Domain.Utilities;
using PRLab.Domain.Value.Enum.System;

namespace PRLab.Tests.DomainTests;

public sealed class UserTests
{
    [Fact]
    public void New_ShouldCreateUser_WithNormalizedNameAndDefaultRole()
    {
        var name = "  Regular user  ";

        var user = User.New(name);

        user.Name.Should().Be(FormatingUtilities.NormalizeName(name));
        user.Role.Should().Be(UserRole.User);
        user.Audit.Should().NotBeNull();
        user.Audit.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void NewCoach_ShouldCreateUser_WithCoachRole()
    {
        var name = "  Coach user  ";

        var user = User.NewCoach(name);

        user.Name.Should().Be(FormatingUtilities.NormalizeName(name));
        user.Role.Should().Be(UserRole.Coach);
        user.Audit.Should().NotBeNull();
        user.Audit.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void NewAdmin_ShouldCreateUser_WithAdminRole()
    {
        var name = "  Admin user  ";

        var user = User.NewAdmin(name);

        user.Name.Should().Be(FormatingUtilities.NormalizeName(name));
        user.Role.Should().Be(UserRole.Admin);
        user.Audit.Should().NotBeNull();
        user.Audit.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Admin_ShouldCreateDefaultAdmin()
    {
        var user = User.Admin();

        user.Id.Should().Be(User.SystemUser.Id);
        user.Name.Should().Be(FormatingUtilities.NormalizeName(User.SystemUser.Name));
        user.Role.Should().Be(UserRole.Admin);
        user.Audit.Should().NotBeNull();
        user.Audit.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Rename_ShouldNormalizeName()
    {
        var initialName = "Initial user";
        var newName = "  Updated user  ";
        var user = User.New(initialName);

        user.Rename(newName);

        user.Name.Should().Be(FormatingUtilities.NormalizeName(newName));
    }

    [Fact]
    public void Rename_ShouldMarkUserAsUpdated()
    {
        var initialName = "Initial user";
        var newName = "Updated user";
        var user = User.New(initialName);
        var previousUpdatedAt = user.Audit.UpdatedAt;

        user.Rename(newName);

        user.Audit.UpdatedAt.Should().NotBe(previousUpdatedAt);
    }

    [Fact]
    public void ChangeRole_ShouldChangeRole_WhenRoleIsDifferent()
    {
        var name = "Regular user";
        var user = User.New(name);

        user.ChangeRole(UserRole.Coach);

        user.Role.Should().Be(UserRole.Coach);
    }

    [Fact]
    public void ChangeRole_ShouldNotMarkUpdated_WhenRoleIsSame()
    {
        var name = "Regular user";
        var user = User.New(name);
        var previousUpdatedAt = user.Audit.UpdatedAt;

        user.ChangeRole(UserRole.User);

        user.Role.Should().Be(UserRole.User);
        user.Audit.UpdatedAt.Should().Be(previousUpdatedAt);
    }

    [Fact]
    public void MarkDeleted_ShouldMarkUserAsDeleted_WhenCalledThroughAuditedInterface()
    {
        var name = "Regular user";
        var user = User.New(name);

        ((IAudited)user).MarkDeleted();

        user.Audit.IsDeleted.Should().BeTrue();
    }
}