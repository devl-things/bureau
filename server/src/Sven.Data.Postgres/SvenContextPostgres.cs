using Microsoft.EntityFrameworkCore;
using Sven.Data.Contexts;
using Sven.Data.Models;
using Sven.Data.Postgres.TypeConfigurations;

namespace Sven.Data.Postgres
{
    internal class SvenContextPostgres : SvenContext
    {
        public SvenContextPostgres(DbContextOptions<SvenContextPostgres> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ClientTypeConfiguration<ClientDb>());

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SvenContextPostgres).Assembly);
        }
    }
}
