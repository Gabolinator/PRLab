using Microsoft.EntityFrameworkCore;
using PRLab.Application.Interface.DB;
using PRLab.Infrastructure.DB.Seeding;

namespace PRLab.Infrastructure.DB.ModelBuilders;

public static class SeedHistoryModelBuilder
{
    public static void CreateSeedHistoryTableModel(this ModelBuilder modelBuilder)
    {
        var seedHistory = modelBuilder.Entity<SeedHistory>();

        seedHistory.ToTable("SeedHistory");

        seedHistory.HasKey(history => history.Name);

        seedHistory.Property(history => history.Name)
            .HasMaxLength(128)
            .IsRequired();

        seedHistory.Property(history => history.AppliedAtUtc)
            .IsRequired();
    }
}