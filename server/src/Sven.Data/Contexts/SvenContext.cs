using Microsoft.EntityFrameworkCore;
using Sven.Data.Models;

namespace Sven.Data.Contexts
{
    public abstract class SvenContext : DbContext
    {
        protected SvenContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<ClientDb> Clients { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ConfigureClient();

            // Provider-agnostic configurations
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(SvenContext).Assembly);
        }
    }
}
