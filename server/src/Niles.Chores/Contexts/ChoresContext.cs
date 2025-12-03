using Microsoft.EntityFrameworkCore;
using Niles.Chores.Models;
using Niles.Chores.TypeConfigurations;

namespace Niles.Chores.Contexts
{
    internal class ChoresContext : DbContext
    {
        public DbSet<ChoreDb> Chores { get; set; } = null!;
        public DbSet<CompletedChoreDb> CompletedChores { get; set; } = null!;
        public DbSet<CriticalChoreDb> CriticalChores { get; set; } = null!;
        public DbSet<HousekeepingDb> Housekeeping { get; set; } = null!;

        public ChoresContext(DbContextOptions<ChoresContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ChoreTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CompletedChoreTypeConfiguration());
            modelBuilder.ApplyConfiguration(new CriticalChoreTypeConfiguration());
            modelBuilder.ApplyConfiguration(new HousekeepingTypeConfiguration());
        }
    }

}
