using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watson.Nodes.Abstractions.Conventions;
using Watson.Nodes.Conventions;
using Watson.Nodes.Models;

namespace Watson.Nodes.TypeConfigurations
{
    internal class NodeAttributeTypeConfiguration : IEntityTypeConfiguration<NodeAttributeDb>
    {
        public void Configure(EntityTypeBuilder<NodeAttributeDb> builder)
        {
            builder.HasKey(x => new { x.NodeId, x.Key, x.Locale });

            builder.Property(x => x.NodeId).IsRequired();

            builder.Property(x => x.Key).HasMaxLength(NodeAttributeConventions.MaxKeyLength).IsRequired();

            builder.Property(x => x.Locale).HasMaxLength(LocaleConventions.MaxLocaleLength).IsRequired();

            builder.Property(x => x.Type).IsRequired();

            builder.Property(x => x.ValueString).IsRequired(false);

            builder.Property(x => x.ValueJson).IsRequired(false);

            builder.Property(x => x.ValueBool).IsRequired(false);

            builder.Property(x => x.RefNodeId).IsRequired(false);

            builder.Property(x => x.ValueDate).IsRequired(false);

            builder.Property(x => x.OrderIndex).IsRequired();

            builder.HasOne(x => x.Node)
                .WithMany()
                .HasForeignKey(x => x.NodeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
