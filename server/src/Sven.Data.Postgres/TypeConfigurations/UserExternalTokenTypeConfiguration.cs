using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sven.Data.Models;

namespace Sven.Data.Postgres.TypeConfigurations
{
    internal class UserExternalTokenTypeConfiguration : IEntityTypeConfiguration<UserExternalTokenDb>
    {
        public void Configure(EntityTypeBuilder<UserExternalTokenDb> builder)
        {
            builder.ToTable("UserExternalTokens");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.UserId).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Provider).HasMaxLength(50).IsRequired();
            builder.Property(x => x.ExternalAccountId).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Scopes).HasMaxLength(2000).IsRequired();
            builder.Property(x => x.AccessToken).HasColumnType("text").IsRequired();
            builder.Property(x => x.RefreshToken).HasColumnType("text");
            builder.Property(x => x.ExpiresAt).HasColumnType("timestamptz").IsRequired();
            builder.Property(x => x.LinkedAt).HasColumnType("timestamptz").IsRequired();
            builder.Property(x => x.LastRefreshedAt).HasColumnType("timestamptz");
            builder.Property(x => x.RequiresReauthorisation).IsRequired();
            builder.HasIndex(x => new { x.UserId, x.Provider, x.ExternalAccountId }).IsUnique();
        }
    }
}
