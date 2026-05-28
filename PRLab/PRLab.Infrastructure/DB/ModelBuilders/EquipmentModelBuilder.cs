using Microsoft.EntityFrameworkCore;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Infrastructure.DB.ModelBuilders;

public static class EquipmentModelBuilder
{
    public static void CreateEquipmentTableModel(this ModelBuilder modelBuilder)
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

    public static void AddEquipmentIndexes(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Equipment>(equipment =>
        {
            equipment.HasIndex(equipment => equipment.Name);

            equipment.HasIndex("DescriptionId");
        });
    }
}