using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sven.Data.Models;
using Sven.Data.TypeConfigurations;

namespace Sven.Data.Postgres.TypeConfigurations
{
    internal class ClientTypeConfiguration<TEntity> : ClientBaseTypeConfiguration<TEntity> where TEntity : ClientDb
    {
        public override void Configure(EntityTypeBuilder<TEntity> builder)
        {
            base.Configure(builder);

            builder.Property(e => e.RefreshTokenLifetime)
                .HasColumnType("interval");
            builder.Property(e => e.IdTokenLifetime)
                .HasColumnType("interval");
            builder.Property(e => e.AccessTokenLifetime)
                .HasColumnType("interval");
        }
    }
}
