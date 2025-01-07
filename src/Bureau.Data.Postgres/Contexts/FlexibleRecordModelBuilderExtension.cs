using Bureau.Data.Postgres.Models;
using Microsoft.EntityFrameworkCore;

namespace Bureau.Data.Postgres.Contexts
{
    internal static class FlexibleRecordModelBuilderExtension
    {
        public static void ConfigureFlexibleRecord(this ModelBuilder modelBuilder) 
        {
            // Configure FlexibleRecord
            modelBuilder.Entity<FlexibleRecord>()
                .HasOne(x => x.Record)
                .WithOne()
                .HasForeignKey<FlexibleRecord>(x => x.Id);

            // Set JSON column type for FlexibleRecord.Data
            modelBuilder.Entity<FlexibleRecord>()
                .Property(x => x.Data)
                .HasColumnType("json");
        }
    }
}
