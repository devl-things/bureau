using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watson.Nodes.Models;

namespace Watson.Nodes.TypeConfigurations
{
    internal class NodeAttributeTypeConfiguration : IEntityTypeConfiguration<NodeAttributeDb>
    {
        public void Configure(EntityTypeBuilder<NodeAttributeDb> builder)
        {
            //TODO
            //builder.ToTable("NodeAttributes");

            // Composite PK to enforce (NodeId, Key, Locale) uniqueness
            builder.HasKey(x => new { x.NodeId, x.Key, x.Locale });

            builder.Property(x => x.Key)
                .HasMaxLength(64)
                .IsRequired();

            builder.Property(x => x.Locale)
                .HasMaxLength(16)
                .IsRequired(false);

            builder.Property(x => x.Type)
                .HasConversion<int>()
                .IsRequired();

            builder.Property(x => x.ValueString)
                .HasMaxLength(512)
                .IsRequired(false);

            //TODO
            //builder.Property(x => x.ValueNumber)
            //    .HasColumnType("decimal(18, 4)")
            //    .IsRequired(false);

            builder.Property(x => x.ValueBool)
                .IsRequired(false);

            //TODO
            //builder.Property(x => x.ValueJson)
            //    .HasColumnType("nvarchar(max)")
            //    .IsRequired(false);

            builder.Property(x => x.RefNodeId)
                .IsRequired(false);

            builder.Property(x => x.ValueDate)
                .IsRequired(false);

            builder.HasOne(x => x.Node)
                .WithMany()
                .HasForeignKey(x => x.NodeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Useful for search matching by label/description etc.
            builder.HasIndex(x => new { x.Key, x.Locale, x.NodeId });

            // Optional: speed LIKE queries on ValueString (not a full-text index though)
            builder.HasIndex(x => new { x.Key, x.Locale });
        }
    }
}
