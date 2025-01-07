using Bureau.Data.Postgres.Models;
using Microsoft.EntityFrameworkCore;

namespace Bureau.Data.Postgres.Contexts
{
    internal static class TermEntryModelBuilderExtension
    {
        public static void ConfigureTermEntry(this ModelBuilder modelBuilder) 
        {
            // Configure TermEntry
            modelBuilder.Entity<TermEntry>()
                .HasOne(x => x.Record)
                .WithOne()
                .HasForeignKey<TermEntry>(x => x.Id);
        }
    }
}
