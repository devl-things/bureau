using Bureau.Data.Postgres.Models;
using Microsoft.EntityFrameworkCore;

namespace Bureau.Data.Postgres.Contexts
{
    internal class BureauContext : DbContext
    {
        public BureauContext(DbContextOptions<BureauContext> options) : base(options)
        {
        }

        public DbSet<Record> Records { get; set; } = null!;
        public DbSet<Edge> Edges { get; set; } = null!;
        public DbSet<TermEntry> TermEntries { get; set; } = null!;
        public DbSet<FlexibleRecord> FlexibleRecords { get; set; } = null!;
        public DbSet<OccurrenceRecord> OccurrenceRecords { get; set; } = null!;
        public DbSet<EnumData> EnumData { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ConfigureRecord();

            modelBuilder.ConfigureEdge();

            modelBuilder.ConfigureTermEntry();

            modelBuilder.ConfigureFlexibleRecord();

            modelBuilder.ConfigureOccurrenceRecord();

            modelBuilder.ConfigureEnumData();

        }
    }
}
