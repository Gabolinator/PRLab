using Microsoft.EntityFrameworkCore;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Infrastructure.DB.ModelBuilders.Entity;

public static class MovementModelBuilder
{
    public static void AddMovementTableModels(this ModelBuilder modelBuilder)
    {
        modelBuilder.CreateMovementTableModel();
        modelBuilder.CreateMovementPatternTagTableModel();
        modelBuilder.CreateMovementMuscleTableModel();
        modelBuilder.CreateMovementEquipmentRequirementTableModel();
    }
    
    public static void CreateMovementTableModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Movement>(movement =>
        {
            movement.ToTable("Movement");

            movement.HasKey(movement => movement.Id);

            movement.Property(movement => movement.Id)
                .HasConversion(
                    movementId => movementId.Value,
                    value => MovementId.FromGuid(value))
                .ValueGeneratedNever();

            movement.Property(movement => movement.Name)
                .HasMaxLength(150)
                .IsRequired();

            movement.Property(movement => movement.NameKey)
                .HasMaxLength(150)
                .IsRequired();

            movement.Property(movement => movement.MovementCategoryId)
                .HasConversion(
                    movementCategoryId => movementCategoryId.Value,
                    value => MovementCategoryId.FromGuid(value))
                .IsRequired();

            movement.Property(movement => movement.VariantOfId)
                .HasConversion<Guid?>(
                    movementId => movementId.HasValue ? movementId.Value.Value : null,
                    value => value.HasValue ? MovementId.FromGuid(value.Value) : null);

            movement.Property(movement => movement.PrimaryPattern)
                .HasConversion<string>()
                .HasMaxLength(80);

            movement.HasOne(movement => movement.Description)
                .WithMany()
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            movement.HasOne(movement => movement.MovementCategory)
                .WithMany()
                .HasForeignKey(movement => movement.MovementCategoryId)
                .OnDelete(DeleteBehavior.Restrict);

            movement.HasOne(movement => movement.VariantOf)
                .WithMany(movement => movement.Variants)
                .HasForeignKey(movement => movement.VariantOfId)
                .OnDelete(DeleteBehavior.Restrict);

            movement.OwnsOne(movement => movement.Audit, audit =>
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

            movement.Navigation(movement => movement.Patterns)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            movement.Navigation(movement => movement.Muscles)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            movement.Navigation(movement => movement.EquipmentRequirements)
                .UsePropertyAccessMode(PropertyAccessMode.Field);

            movement.Navigation(movement => movement.Variants)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });
    }

    public static void CreateMovementPatternTagTableModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MovementPatternTag>(movementPatternTag =>
        {
            movementPatternTag.ToTable("MovementPatternTag");

            movementPatternTag.HasKey(movementPatternTag => new
            {
                movementPatternTag.MovementId,
                movementPatternTag.Pattern
            });

            movementPatternTag.Property(movementPatternTag => movementPatternTag.MovementId)
                .HasConversion(
                    movementId => movementId.Value,
                    value => MovementId.FromGuid(value))
                .IsRequired();

            movementPatternTag.Property(movementPatternTag => movementPatternTag.Pattern)
                .HasConversion<string>()
                .HasMaxLength(80)
                .IsRequired();

            movementPatternTag.HasOne<Movement>()
                .WithMany(movement => movement.Patterns)
                .HasForeignKey(movementPatternTag => movementPatternTag.MovementId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    public static void CreateMovementMuscleTableModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MovementMuscle>(movementMuscle =>
        {
            movementMuscle.ToTable("MovementMuscle");

            movementMuscle.HasKey(movementMuscle => new
            {
                movementMuscle.MovementId,
                movementMuscle.MuscleId
            });

            movementMuscle.Property(movementMuscle => movementMuscle.MovementId)
                .HasConversion(
                    movementId => movementId.Value,
                    value => MovementId.FromGuid(value))
                .IsRequired();

            movementMuscle.Property(movementMuscle => movementMuscle.MuscleId)
                .HasConversion(
                    muscleId => muscleId.Value,
                    value => MuscleId.FromGuid(value))
                .IsRequired();

            movementMuscle.Property(movementMuscle => movementMuscle.Role)
                .HasConversion<string>()
                .HasMaxLength(80)
                .IsRequired();

            movementMuscle.HasOne(movementMuscle => movementMuscle.Movement)
                .WithMany(movement => movement.Muscles)
                .HasForeignKey(movementMuscle => movementMuscle.MovementId)
                .OnDelete(DeleteBehavior.Cascade);

            movementMuscle.HasOne(movementMuscle => movementMuscle.Muscle)
                .WithMany()
                .HasForeignKey(movementMuscle => movementMuscle.MuscleId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public static void CreateMovementEquipmentRequirementTableModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<MovementEquipmentRequirement>(requirement =>
        {
            requirement.ToTable("MovementEquipmentRequirement");

            requirement.HasKey(requirement => new
            {
                requirement.MovementId,
                requirement.EquipmentId,
                requirement.GroupKey,
                requirement.Kind
            });

            requirement.Property(requirement => requirement.MovementId)
                .HasConversion(
                    movementId => movementId.Value,
                    value => MovementId.FromGuid(value))
                .IsRequired();

            requirement.Property(requirement => requirement.EquipmentId)
                .HasConversion(
                    equipmentId => equipmentId.Value,
                    value => EquipmentId.FromGuid(value))
                .IsRequired();

            requirement.Property(requirement => requirement.GroupKey)
                .HasMaxLength(100)
                .IsRequired();

            requirement.Property(requirement => requirement.Kind)
                .HasConversion<string>()
                .HasMaxLength(80)
                .IsRequired();

            requirement.HasOne<Movement>()
                .WithMany(movement => movement.EquipmentRequirements)
                .HasForeignKey(requirement => requirement.MovementId)
                .OnDelete(DeleteBehavior.Cascade);

            requirement.HasOne(requirement => requirement.Equipment)
                .WithMany()
                .HasForeignKey(requirement => requirement.EquipmentId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }

    public static void AddMovementIndexes(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Movement>(movement =>
        {
            movement.HasIndex(movement => movement.Name)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");

            movement.HasIndex(movement => movement.NameKey)
                .IsUnique()
                .HasFilter("\"IsDeleted\" = false");

            movement.HasIndex(movement => movement.MovementCategoryId);

            movement.HasIndex(movement => movement.VariantOfId);

            movement.HasIndex(movement => movement.PrimaryPattern);

            movement.HasIndex("DescriptionId");
        });

        modelBuilder.Entity<MovementPatternTag>(movementPatternTag =>
        {
            movementPatternTag.HasIndex(movementPatternTag => movementPatternTag.Pattern);
        });

        modelBuilder.Entity<MovementMuscle>(movementMuscle =>
        {
            movementMuscle.HasIndex(movementMuscle => movementMuscle.MuscleId);
            movementMuscle.HasIndex(movementMuscle => movementMuscle.Role);
        });

        modelBuilder.Entity<MovementEquipmentRequirement>(movementEquipment =>
        {
            movementEquipment.HasIndex(movementEquipment => movementEquipment.EquipmentId);
        });
    }
}