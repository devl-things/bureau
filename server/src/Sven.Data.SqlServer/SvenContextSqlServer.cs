using Microsoft.EntityFrameworkCore;
using Sven.Data.Contexts;
using Sven.Data.Models;
using Sven.Data.SqlServer.TypeConfigurations;

namespace Sven.Data.SqlServer
{
    internal class SvenContextSqlServer : SvenContext
    {
        public SvenContextSqlServer(DbContextOptions<SvenContextSqlServer> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.ApplyConfiguration(new ClientTypeConfiguration<ClientDb>());
            modelBuilder.ApplyConfiguration(new UserTypeConfiguration<UserDb>());

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SvenContextSqlServer).Assembly);
        }
    }
}
