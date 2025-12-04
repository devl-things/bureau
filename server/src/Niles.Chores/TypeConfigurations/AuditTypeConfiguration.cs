using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Niles.Chores.Models;

namespace Niles.Chores.TypeConfigurations
{
    internal class AuditTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : class, IAuditable
    {
        public virtual void Configure(EntityTypeBuilder<TEntity> builder)
        {
            builder.Property(x => x.CreatedAt).HasColumnType("datetimeoffset").IsRequired();
            builder.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
            builder.Property(x => x.UpdatedAt).HasColumnType("datetimeoffset").IsRequired();
            builder.Property(x => x.UpdatedBy).HasMaxLength(100).IsRequired();
        }
    }
}
