using Microsoft.EntityFrameworkCore;
using Watson.Nodes.Models;
using Watson.Nodes.TypeConfigurations;

namespace Watson.Nodes.Contexts
{
    internal class NodesContext : DbContext
    {
        public DbSet<NodeDb> Nodes { get; set; } = null!;
        public DbSet<NodeAttributeDb> NodeAttributes { get; set; } = null!;

        public NodesContext(DbContextOptions<NodesContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new NodeTypeConfiguration());
            modelBuilder.ApplyConfiguration(new NodeAttributeTypeConfiguration());
        }
    }
}
