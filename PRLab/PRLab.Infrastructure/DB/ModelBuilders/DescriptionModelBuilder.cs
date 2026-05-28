using Microsoft.EntityFrameworkCore;
using PRLab.Domain.Model.Join;
using PRLab.Domain.Value.Identifier;

namespace PRLab.Infrastructure.DB.ModelBuilders;


public static class DescriptionModelBuilder
{
    public static void AddDescriptionIndexes(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Description>(description =>
        {
            description.HasIndex(description => description.DefaultLanguageCode);
        });

        modelBuilder.Entity<DescriptionTranslation>(translation =>
        {
            translation.HasIndex(translation => translation.DescriptionId);

            translation.HasIndex(translation => new
                {
                    translation.DescriptionId,
                    translation.LanguageCode
                })
                .IsUnique();

            translation.HasIndex(translation => new
            {
                translation.LanguageCode,
                translation.Content
            });
        });
    }

    public static void CreateDescriptionTableModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Description>(description =>
        {
            description.ToTable("Descriptions");

            description.HasKey(description => description.Id);

            description.Property(description => description.Id)
                .HasConversion(
                    descriptionId => descriptionId.Value,
                    value => DescriptionId.FromGuid(value))
                .ValueGeneratedNever();

            description.Property(description => description.DefaultLanguageCode)
                .HasMaxLength(10)
                .IsRequired();

            description.HasMany(description => description.Translations)
                .WithOne()
                .HasForeignKey(translation => translation.DescriptionId)
                .OnDelete(DeleteBehavior.Cascade);

            description.Navigation(description => description.Translations)
                .UsePropertyAccessMode(PropertyAccessMode.Field);
        });
    }

    public static void CreateDescriptionTranslationTableModel(this ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DescriptionTranslation>(translation =>
        {
            translation.ToTable("DescriptionTranslations");

            translation.HasKey(translation => translation.Id);

            translation.Property(translation => translation.Id)
                .HasConversion(
                    descriptionTranslationId => descriptionTranslationId.Value,
                    value => DescriptionTranslationId.FromGuid(value))
                .ValueGeneratedNever();

            translation.Property(translation => translation.DescriptionId)
                .HasConversion(
                    descriptionId => descriptionId.Value,
                    value => DescriptionId.FromGuid(value))
                .IsRequired();

            translation.Property(translation => translation.LanguageCode)
                .HasMaxLength(10)
                .IsRequired();

            translation.Property(translation => translation.Content)
                .HasMaxLength(2000);

            translation.HasIndex(translation => new
            {
                translation.DescriptionId,
                translation.LanguageCode
            })
            .IsUnique();

            translation.HasIndex(translation => new
            {
                translation.LanguageCode,
                translation.Content
            });
        });
    }
}