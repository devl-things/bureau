using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sven.Data.Models;

namespace Sven.Data.Postgres.TypeConfigurations
{
    internal class RefreshTokenTypeConfiguration : IEntityTypeConfiguration<RefreshTokenDb>
    {
        public void Configure(EntityTypeBuilder<RefreshTokenDb> builder)
        {
            builder.ToTable("RefreshTokens");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.Token).HasMaxLength(100).IsRequired();
            builder.Property(x => x.ClientId).HasMaxLength(100).IsRequired();
            builder.Property(x => x.RedirectUri).HasMaxLength(500).IsRequired();
            builder.Property(x => x.Scope).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.Nonce).HasMaxLength(500);
            builder.Property(x => x.Claims).HasColumnType("text").IsRequired();
            builder.Property(x => x.ExpiresAt).HasColumnType("timestamptz").IsRequired();
            builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();
            builder.HasIndex(x => x.Token).IsUnique();
        }
    }
}
