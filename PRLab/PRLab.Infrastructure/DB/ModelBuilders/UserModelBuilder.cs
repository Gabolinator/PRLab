using Microsoft.EntityFrameworkCore;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Infrastructure.DB.ModelBuilders;

public static class UserModelBuilder
{
    public static void AddUserTableModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.ToTable("Users");

            user.HasKey(user => user.Id);

            user.Property(user => user.Id)
                .HasConversion(
                    userId => userId.Value,
                    value => UserId.FromGuid(value))
                .ValueGeneratedNever();

            user.Property(user => user.Name)
                .HasMaxLength(150)
                .IsRequired();

            user.Property(user => user.Role)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

            user.OwnsOne(user => user.Audit, audit =>
            {
                audit.Property(auditInfo => auditInfo.CreatedAt)
                    .IsRequired();

                audit.Property(auditInfo => auditInfo.CreatedBy)
                    .HasConversion<Guid>(
                        userId => userId.Value,
                        value => UserId.FromGuid(value))
                    .IsRequired();

                audit.Property(auditInfo => auditInfo.UpdatedAt);

                audit.Property(auditInfo => auditInfo.UpdatedBy)
                    .HasConversion<Guid?>(
                        userId => userId.HasValue ? userId.Value.Value : null,
                        value => value.HasValue ? UserId.FromGuid(value.Value) : null);

                audit.Property(auditInfo => auditInfo.IsDeleted)
                    .IsRequired();

                audit.HasIndex(auditInfo => auditInfo.IsDeleted);

                audit.Property(auditInfo => auditInfo.DeletedAt);

                audit.Property(auditInfo => auditInfo.DeletedBy)
                    .HasConversion<Guid?>(
                        userId => userId.HasValue ? userId.Value.Value : null,
                        value => value.HasValue ? UserId.FromGuid(value.Value) : null);
            });
        });
    }

    public static void AddUserIndexes(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(user =>
        {
            user.HasIndex(user => user.Name);

            user.HasIndex(user => user.Role);
        });
    }
}