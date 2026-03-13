using Microsoft.EntityFrameworkCore;
using Watson.Nodes.Contexts;

namespace Watson.Nodes.Data.SqlServer.Contexts
{
    internal sealed class SqlServerNodesContext : NodesContext
    {
        public SqlServerNodesContext(DbContextOptions<SqlServerNodesContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(SqlServerNodesContext).Assembly);
        }
    }
}
