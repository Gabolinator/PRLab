using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB;
using PRLab.Application.Models.DB.Seeding;

namespace PRLab.Infrastructure.DB.ModelBuilders;

public static class SeedHistoryModelBuilder
{
    public static void AddSeedHistoryTableModel(this ModelBuilder modelBuilder)
    {
        var seedHistory = modelBuilder.Entity<SeedHistory>();

        seedHistory.ToTable("SeedHistory");

        seedHistory.HasKey(history => history.Id);

        seedHistory.Property(history => history.Id)
            .HasConversion(
                id => id.Value,
                value => SeedHistoryId.FromGuid(value))
            .ValueGeneratedNever();

        seedHistory.Property(history => history.Name)
            .HasMaxLength(128)
            .IsRequired();

        seedHistory.Property(history => history.Version)
            .HasMaxLength(32)
            .IsRequired();

        seedHistory.Property(history => history.AppliedAtUtc)
            .IsRequired();

        seedHistory.HasIndex(history => new
            {
                history.Name,
                history.Version
            })
            .IsUnique();
    }
}