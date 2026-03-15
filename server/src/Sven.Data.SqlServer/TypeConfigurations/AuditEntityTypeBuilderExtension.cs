using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sven.Data.Models;

namespace Sven.Data.SqlServer.TypeConfigurations
{
    internal static class AuditEntityTypeBuilderExtension
    {
        internal static void ConfigureAuditFields<TEntity>(this EntityTypeBuilder<TEntity> builder) where TEntity : class, IAuditable
        {
            builder.Property(e => e.CreatedAt)
                .HasColumnType("datetimeoffset");
            builder.Property(e => e.UpdatedAt)
                .HasColumnType("datetimeoffset");
        }
    }
}
