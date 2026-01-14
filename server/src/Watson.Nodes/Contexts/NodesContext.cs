using Microsoft.EntityFrameworkCore;
using Watson.Nodes.Models;

namespace Watson.Nodes.Contexts
{
    internal abstract class NodesContext : DbContext
    {
        public DbSet<NodeDb> Nodes { get; set; } = null!;
        public DbSet<NodeAttributeDb> NodeAttributes { get; set; } = null!;

        internal NodesContext(DbContextOptions options) : base(options)
        {
        }
    }
}
