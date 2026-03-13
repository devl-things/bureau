using Bureau.EntityFrameworkCore.SqlServer.TypeConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watson.Nodes.Models;

namespace Watson.Nodes.Data.SqlServer.TypeConfigurations
{
    internal sealed class NodeEdgeTypeConfiguration : IEntityTypeConfiguration<NodeEdgeDb>
    {
        public void Configure(EntityTypeBuilder<NodeEdgeDb> builder)
        {
            new Nodes.TypeConfigurations.NodeEdgeTypeConfiguration().Configure(builder);

            builder.ToTable("NodeEdges");

            builder.ConfigureAuditFields();

            builder.HasIndex(x => new { x.SourceNodeId, x.Purpose });
            builder.HasIndex(x => new { x.TargetNodeId, x.Purpose });
        }
    }
}
