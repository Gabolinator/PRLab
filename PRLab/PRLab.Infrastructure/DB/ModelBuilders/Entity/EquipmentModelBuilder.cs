using Microsoft.EntityFrameworkCore;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Infrastructure.DB.ModelBuilders.Entity;

public static class EquipmentModelBuilder
{
    public static void AddEquipmentTableModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Equipment>(equipment =>
        {
            equipment.ToTable("Equipment");

            equipment.HasKey(equipment => equipment.Id);

            equipment.Property(equipment => equipment.Id)
                .HasConversion(
                    equipmentId => equipmentId.Value,
                    value => EquipmentId.FromGuid(value))
                .ValueGeneratedNever();

            equipment.Property(equipment => equipment.Name)
                .HasMaxLength(150)
                .IsRequired();
            
            equipment.Property(equipment => equipment.NameKey)
                .HasMaxLength(150)
                .IsRequired();

            equipment.HasOne(equipment => equipment.Description)
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            equipment.OwnsOne(equipment => equipment.Audit, audit =>
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
            
            equipment.OwnsOne(equipment => equipment.Ownership, ownership =>
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
                    .HasDatabaseName("IX_Equipment_DataOrigin");

                ownership.HasIndex(ownershipInfo => ownershipInfo.OwnerUserId)
                    .HasDatabaseName("IX_Equipment_OwnerUserId");
            });
            
        });
    }

    public static void AddEquipmentIndexes(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Equipment>(equipment =>
        {
            equipment.HasIndex(equipment => equipment.Name)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");

            equipment.HasIndex(equipment => equipment.NameKey)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");

            equipment.HasIndex("DescriptionId");
        });
    }
}