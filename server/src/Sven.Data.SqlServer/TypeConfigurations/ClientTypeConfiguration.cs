using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sven.Data.Models;
using Sven.Data.TypeConfigurations;

namespace Sven.Data.SqlServer.TypeConfigurations
{
    internal class ClientTypeConfiguration<TEntity> : ClientBaseTypeConfiguration<TEntity> where TEntity : ClientDb
    {
        public override void Configure(EntityTypeBuilder<TEntity> builder)
        {
            base.Configure(builder);
            builder.Property(e => e.RefreshTokenLifetime)
                .HasColumnType("bigint")
                .HasConversion(
                    v => v.HasValue ? v.Value.Ticks : (long?)null,
                    v => v.HasValue ? TimeSpan.FromTicks(v.Value) : null
                );
            builder.Property(e => e.IdTokenLifetime)
                .HasColumnType("bigint")
                .HasConversion(
                    v => v.HasValue ? v.Value.Ticks : (long?)null,
                    v => v.HasValue ? TimeSpan.FromTicks(v.Value) : null
                );
            builder.Property(e => e.AccessTokenLifetime)
                .HasColumnType("bigint")
                .HasConversion(
                    v => v.HasValue ? v.Value.Ticks : (long?)null,
                    v => v.HasValue ? TimeSpan.FromTicks(v.Value) : null
                );

            builder.ConfigureAuditFields();
        }
    }
}
