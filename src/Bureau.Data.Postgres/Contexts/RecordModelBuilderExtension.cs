using Bureau.Data.Postgres.Models;
using Microsoft.EntityFrameworkCore;

namespace Bureau.Data.Postgres.Contexts
{
    internal static class RecordModelBuilderExtension
    {
        public static void ConfigureRecord(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Record>()
                .HasKey(x => x.Id);

            modelBuilder.Entity<Record>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd(); // Automatically generate Guid on add

            // Configure lengths and column types
            modelBuilder.Entity<Record>()
                .Property(x => x.CreatedBy)
                .HasMaxLength(40);

            modelBuilder.Entity<Record>()
                .Property(x => x.UpdatedBy)
                .HasMaxLength(40);

            modelBuilder.Entity<Record>()
                .Property(x => x.ProviderName)
                .HasMaxLength(20);

            modelBuilder.Entity<Record>()
                .Property(x => x.ExternalId)
                .HasMaxLength(40);

            modelBuilder.Entity<Record>()
                .Property(x => x.CreatedAt)
                .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<Record>()
                .Property(x => x.UpdatedAt)
                .HasColumnType("timestamp with time zone");
        }
    }
}
