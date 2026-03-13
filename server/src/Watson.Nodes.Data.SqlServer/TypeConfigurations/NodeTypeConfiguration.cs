using Bureau.EntityFrameworkCore.SqlServer.TypeConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watson.Nodes.Models;

namespace Watson.Nodes.Data.SqlServer.TypeConfigurations
{
    internal sealed class NodeTypeConfiguration : IEntityTypeConfiguration<NodeDb>
    {
        public void Configure(EntityTypeBuilder<NodeDb> builder)
        {
            new Nodes.TypeConfigurations.NodeTypeConfiguration().Configure(builder);

            builder.ToTable("Nodes");

            builder.ConfigureAuditFields();

            builder.Property(x => x.CreatedSequence).UseIdentityColumn();

            // Enums are : byte => store as tinyint explicitly
            builder.Property(x => x.Kind).HasColumnType("tinyint");

            builder.Property(x => x.Status).HasColumnType("tinyint");

            // Helpful query patterns
            builder.HasIndex(x => new { x.Kind, x.Scope, x.CanonicalKey }).IsUnique();
            builder.HasIndex(x => new { x.Kind, x.Scope, x.CreatedSequence });

        }
    }
}
