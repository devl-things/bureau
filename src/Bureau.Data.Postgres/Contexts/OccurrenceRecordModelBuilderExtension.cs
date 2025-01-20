using Bureau.Data.Postgres.Models;
using Microsoft.EntityFrameworkCore;

namespace Bureau.Data.Postgres.Contexts
{
    internal static class OccurrenceRecordModelBuilderExtension
    {
        public static void ConfigureOccurrenceRecord(this ModelBuilder modelBuilder)
        {
            // Configure FlexibleRecord
            modelBuilder.Entity<OccurrenceRecord>()
                .HasOne(x => x.Record)
                .WithOne()
                .HasForeignKey<OccurrenceRecord>(x => x.Id);
            
            modelBuilder.Entity<OccurrenceRecord>()
                .Property(x => x.StartDate)
                .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<OccurrenceRecord>()
                .Property(x => x.EndDate)
                .HasColumnType("timestamp with time zone");

            modelBuilder.Entity<OccurrenceRecord>()
                .Property(x => x.Until)
                .HasColumnType("timestamp with time zone");
        }
    }
}
