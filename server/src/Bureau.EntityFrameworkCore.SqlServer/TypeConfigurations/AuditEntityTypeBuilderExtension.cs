using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Bureau.EntityFrameworkCore.SqlServer.TypeConfigurations
{
    public static class AuditEntityTypeBuilderExtension
    {
        public static void ConfigureAuditFields<TEntity>(this EntityTypeBuilder<TEntity> builder) where TEntity : class, IAuditable
        {
            builder.Property(e => e.CreatedAt).HasColumnType("datetimeoffset");
            builder.Property(e => e.UpdatedAt).HasColumnType("datetimeoffset");
        }
    }
}
