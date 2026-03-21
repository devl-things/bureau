using Microsoft.EntityFrameworkCore;
using Sven.Data.Contexts;

namespace Sven.Data.Postgres.Contexts
{
    internal sealed class SvenContextPostgres : SvenContext
    {
        public SvenContextPostgres(DbContextOptions<SvenContextPostgres> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SvenContextPostgres).Assembly);
        }
    }
}
