using Microsoft.EntityFrameworkCore;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Infrastructure.DB.ModelBuilders.Entity;

public static class MovementCategoryModelBuilder
{
    public static void AddMovementCategoryTableModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MovementCategory>(movementCategory =>
        {
            movementCategory.ToTable("MovementCategory");

            movementCategory.HasKey(movementCategory => movementCategory.Id);

            movementCategory.Property(movementCategory => movementCategory.Id)
                .HasConversion(
                    movementCategoryId => movementCategoryId.Value,
                    value => MovementCategoryId.FromGuid(value))
                .ValueGeneratedNever();

            movementCategory.Property(movementCategory => movementCategory.Name)
                .HasMaxLength(150)
                .IsRequired();

            movementCategory.Property(movementCategory => movementCategory.NameKey)
                .HasMaxLength(150)
                .IsRequired();

            movementCategory.Property(movementCategory => movementCategory.BaseMovementCategory)
                .HasConversion<string>()
                .HasMaxLength(80)
                .IsRequired();

            movementCategory.HasOne(movementCategory => movementCategory.Description)
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            movementCategory.OwnsOne(movementCategory => movementCategory.Audit, audit =>
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
                    .HasColumnName("IsDeleted")
                    .IsRequired();

                audit.HasIndex(auditInfo => auditInfo.IsDeleted);

                audit.Property(auditInfo => auditInfo.DeletedAt);

                audit.Property(auditInfo => auditInfo.DeletedBy)
                    .HasConversion<Guid?>(
                        userId => userId.HasValue ? userId.Value.Value : null,
                        value => value.HasValue ? UserId.FromGuid(value.Value) : null);
            });

            movementCategory.OwnsOne(movementCategory => movementCategory.Ownership, ownership =>
            {
                ownership.Property(ownershipInfo => ownershipInfo.Origin)
                    .HasColumnName("DataOrigin")
                    .HasConversion<string>()
                    .HasMaxLength(50)
                    .IsRequired();

                ownership.Property(ownershipInfo => ownershipInfo.OwnerUserId)
                    .HasColumnName("OwnerUserId")
                    .HasConversion<Guid?>(
                        userId => userId.HasValue ? userId.Value.Value : null,
                        value => value.HasValue ? UserId.FromGuid(value.Value) : null);
                
                ownership.HasIndex(ownershipInfo => ownershipInfo.Origin)
                    .HasDatabaseName("IX_MovementCategory_DataOrigin");

                ownership.HasIndex(ownershipInfo => ownershipInfo.OwnerUserId)
                    .HasDatabaseName("IX_MovementCategory_OwnerUserId");
            });
        });
    }

    public static void AddMovementCategoryIndexes(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MovementCategory>(movementCategory =>
        {
            movementCategory.HasIndex(movementCategory => movementCategory.Name)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");

            movementCategory.HasIndex(movementCategory => movementCategory.NameKey)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");

            movementCategory.HasIndex(movementCategory => movementCategory.BaseMovementCategory);

            movementCategory.HasIndex("DescriptionId");
        });
    }
}