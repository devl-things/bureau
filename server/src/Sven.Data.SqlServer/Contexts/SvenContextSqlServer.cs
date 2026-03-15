using Microsoft.EntityFrameworkCore;
using Sven.Data.Contexts;

namespace Sven.Data.SqlServer.Contexts
{
    internal sealed class SvenContextSqlServer : SvenContext
    {
        public SvenContextSqlServer(DbContextOptions<SvenContextSqlServer> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SvenContextSqlServer).Assembly);
        }
    }
}
