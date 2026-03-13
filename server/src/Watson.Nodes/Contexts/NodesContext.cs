using Microsoft.EntityFrameworkCore;
using Watson.Nodes.Models;

namespace Watson.Nodes.Contexts
{
    internal abstract class NodesContext : DbContext
    {
        public DbSet<NodeDb> Nodes { get; set; } = null!;
        public DbSet<NodeAttributeDb> NodeAttributes { get; set; } = null!;
        public DbSet<NodeEdgeDb> NodeEdges { get; set; } = null!;

        protected NodesContext(DbContextOptions options) : base(options)
        {
        }
    }
}
