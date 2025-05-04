using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sven.Data.Models;

namespace Sven.Data.TypeConfigurations
{
    public class AuditTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : class, IAuditable
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.Property(x => x.CreatedAt)
                .IsRequired();
            builder.Property(x => x.CreatedBy)
                .HasMaxLength(100)
                .IsRequired();
            builder.Property(x => x.UpdatedAt)
                .IsRequired();
            builder.Property(x => x.UpdatedBy)
                .HasMaxLength(100)
                .IsRequired();
        }
    }
}
