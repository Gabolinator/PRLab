using Microsoft.EntityFrameworkCore;
using PRLab.Domain.Model.Entity;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Model.Value.Identifier;

namespace PRLab.Infrastructure.DB.ModelBuilders.Entity;

internal static class AliasModelBuilder
{
    public static void AddAliasTableModels(this ModelBuilder modelBuilder)
    {
        var aliases = modelBuilder.Entity<Alias>();
        aliases.ToTable("Alias");
        aliases.HasKey(alias => alias.Id);
        aliases.Property(alias => alias.Id)
            .HasConversion(id => id.Value, value => AliasId.FromGuid(value))
            .ValueGeneratedNever();
        aliases.Property(alias => alias.Value).HasMaxLength(Alias.MaxValueLength).IsRequired();
        aliases.Property(alias => alias.ValueKey).HasMaxLength(Alias.MaxValueLength).IsRequired();
        aliases.HasIndex(alias => alias.ValueKey);
        aliases.HasAlternateKey(alias => new { alias.Id, alias.ValueKey });

        AddAliasRelation<Muscle, MuscleAlias>(modelBuilder, nameof(MuscleAlias.MuscleId));
        AddAliasRelation<Movement, MovementAlias>(modelBuilder, nameof(MovementAlias.MovementId));
        AddAliasRelation<Exercise, ExerciseAlias>(modelBuilder, nameof(ExerciseAlias.ExerciseId));
        AddAliasRelation<Equipment, EquipmentAlias>(modelBuilder, nameof(EquipmentAlias.EquipmentId));
    }

    private static void AddAliasRelation<TOwner, TRelation>(ModelBuilder modelBuilder, string ownerId)
        where TOwner : class
        where TRelation : class
    {
        const string aliasId = nameof(EquipmentAlias.AliasId);
        const string aliasValueKey = nameof(EquipmentAlias.AliasValueKey);
        const string ownerNavigation = nameof(Equipment.Aliases);

        var relation = modelBuilder.Entity<TRelation>();
        relation.ToTable(typeof(TRelation).Name);
        relation.HasKey(ownerId, aliasId);
        relation.Property<string>(aliasValueKey).HasMaxLength(Alias.MaxValueLength).IsRequired();
        relation.HasIndex(aliasValueKey);
        relation.HasIndex(ownerId, aliasValueKey).IsUnique();

        relation.HasOne<TOwner>()
            .WithMany(ownerNavigation)
            .HasForeignKey(ownerId)
            .OnDelete(DeleteBehavior.Cascade);
        relation.HasOne<Alias>(nameof(EquipmentAlias.Alias))
            .WithMany()
            .HasForeignKey(aliasId, aliasValueKey)
            .HasPrincipalKey(nameof(Alias.Id), nameof(Alias.ValueKey))
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<TOwner>().Navigation(ownerNavigation)
            .HasField("aliases")
            .UsePropertyAccessMode(PropertyAccessMode.Field);
    }
}
