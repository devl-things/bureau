using Bureau.EntityFrameworkCore.TypeConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watson.Nodes.Abstractions.Conventions;
using Watson.Nodes.Models;

namespace Watson.Nodes.TypeConfigurations
{
    internal class NodeTypeConfiguration : AuditTypeConfiguration<NodeDb>, IEntityTypeConfiguration<NodeDb>
    {
        public override void Configure(EntityTypeBuilder<NodeDb> builder)
        {
            base.Configure(builder);

            builder.HasKey(x => x.NodeId);
            builder.Property(x => x.NodeId).ValueGeneratedNever();

            builder.Property(x => x.CreatedSequence).ValueGeneratedOnAdd();

            builder.Property(x => x.Kind).IsRequired();

            builder.Property(x => x.Scope).HasMaxLength(ScopeConventions.MaxScopeLength).IsRequired();

            builder.Property(x => x.CanonicalKey).HasMaxLength(CanonicalKeyConventions.MaxCanonicalKeyLength).IsRequired(false);

            builder.Property(x => x.Status).IsRequired();

            builder.Property(x => x.Version).IsRequired();

            builder.HasMany(x => x.NodeAttributes)
                .WithOne(x => x.Node)
                .HasForeignKey(x => x.NodeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
