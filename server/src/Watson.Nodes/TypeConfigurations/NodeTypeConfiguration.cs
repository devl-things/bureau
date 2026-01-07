using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watson.Nodes.Models;

namespace Watson.Nodes.TypeConfigurations
{
    internal class NodeTypeConfiguration : IEntityTypeConfiguration<NodeDb>
    {
        public void Configure(EntityTypeBuilder<NodeDb> builder)
        {
            //TODO
            //builder.ToTable("Nodes");

            builder.HasKey(x => x.NodeId);

            builder.Property(x => x.NodeId)
                .ValueGeneratedNever();

            // Cursor paging key for SearchAsync: BIGINT IDENTITY
            builder.Property(x => x.CreatedSequence)
                .ValueGeneratedOnAdd();

            builder.HasIndex(x => x.CreatedSequence);

            builder.Property(x => x.Kind)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Scope)
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(x => x.CanonicalKey)
                .HasMaxLength(128)
                .IsRequired(false);

            builder.Property(x => x.Status)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.Version)
                .IsRequired();

            // Uniqueness for canonical key (when present).
            // NOTE: SQL Server unique index allows multiple NULLs, which is what we want.
            builder.HasIndex(x => new { x.Kind, x.Scope, x.CanonicalKey })
                .IsUnique();

            // Main paging index for search
            builder.HasIndex(x => new { x.Kind, x.Scope, x.CreatedSequence });
        }
    }
}
