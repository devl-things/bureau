using Bureau.EntityFrameworkCore.SqlServer.TypeConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Watson.Nodes.Abstractions.Conventions;
using Watson.Nodes.Models;

namespace Watson.Nodes.Data.SqlServer.TypeConfigurations
{
    internal sealed class NodeAttributeTypeConfiguration : IEntityTypeConfiguration<NodeAttributeDb>
    {
        public void Configure(EntityTypeBuilder<NodeAttributeDb> builder)
        {
            new Nodes.TypeConfigurations.NodeAttributeTypeConfiguration().Configure(builder);

            builder.ToTable("NodeAttributes");

            builder.Property(x => x.Locale).HasDefaultValue(LocaleConventions.DefaultLocale);

            builder.ConfigureAuditFields();

            builder.Property(x => x.Type).HasColumnType("tinyint");

            builder.Property(x => x.ValueString).HasColumnType("nvarchar(max)");

            builder.Property(x => x.ValueJson).HasColumnType("nvarchar(max)");

            builder.Property(x => x.ValueNumber).HasColumnType("decimal(18,4)");

            builder.Property(x => x.ValueDate).HasColumnType("datetimeoffset");

            // Useful for search matching by label/description etc.
            builder.HasIndex(x => new { x.NodeId, x.Key, x.Locale });
            builder.HasIndex(x => new { x.NodeId, x.Key, x.Locale, x.OrderIndex });

            // Helps when you filter by (Key, Locale) before matching ValueString / etc.
            builder.HasIndex(x => new { x.Key, x.Locale, x.Type, x.NodeId }).IncludeProperties(nameof(NodeAttributeDb.ValueString), nameof(NodeAttributeDb.ValueJson));

            builder.HasIndex(x => new { x.Key, x.Locale, x.ValueBool });
            builder.HasIndex(x => new { x.Key, x.Locale, x.ValueNumber });
            builder.HasIndex(x => new { x.Key, x.Locale, x.RefNodeId });
            builder.HasIndex(x => new { x.Key, x.Locale, x.ValueDate });
        }
    }
}
