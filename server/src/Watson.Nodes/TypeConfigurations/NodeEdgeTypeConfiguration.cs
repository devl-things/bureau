using Bureau.EntityFrameworkCore.TypeConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watson.Nodes.Abstractions.Conventions;
using Watson.Nodes.Models;

namespace Watson.Nodes.TypeConfigurations
{
    internal class NodeEdgeTypeConfiguration : AuditTypeConfiguration<NodeEdgeDb>, IEntityTypeConfiguration<NodeEdgeDb>
    {
        public override void Configure(EntityTypeBuilder<NodeEdgeDb> builder)
        {
            base.Configure(builder);

            builder.HasKey(x => new { x.SourceNodeId, x.TargetNodeId, x.Purpose });

            builder.Property(x => x.SourceNodeId).IsRequired();
            builder.Property(x => x.TargetNodeId).IsRequired();

            builder.Property(x => x.Purpose).HasMaxLength(EdgeConventions.MaxPurposeLength).IsRequired();

            builder.Property(x => x.OrderIndex).IsRequired();

            builder.HasOne(x => x.SourceNode)
                .WithMany()
                .HasForeignKey(x => x.SourceNodeId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.TargetNode)
                .WithMany()
                .HasForeignKey(x => x.TargetNodeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
