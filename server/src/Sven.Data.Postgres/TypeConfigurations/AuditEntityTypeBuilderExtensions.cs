using Bureau;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Sven.Data.Postgres.TypeConfigurations
{
    internal static class AuditEntityTypeBuilderExtensions
    {
        internal static void ConfigureAuditFields<TEntity>(this EntityTypeBuilder<TEntity> builder) where TEntity : class, IAuditable
        {
            builder.Property(e => e.CreatedAt).HasColumnType("timestamptz");
            builder.Property(e => e.UpdatedAt).HasColumnType("timestamptz");
        }
    }
}
