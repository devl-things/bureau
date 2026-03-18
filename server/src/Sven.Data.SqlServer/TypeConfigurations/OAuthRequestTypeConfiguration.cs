using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sven.Data.Models;

namespace Sven.Data.SqlServer.TypeConfigurations
{
    internal sealed class OAuthRequestTypeConfiguration : IEntityTypeConfiguration<OAuthRequestDb>
    {
        public void Configure(EntityTypeBuilder<OAuthRequestDb> builder)
        {
            builder.ToTable("OAuthRequests");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.PkceKey).HasMaxLength(64).IsRequired();
            builder.HasIndex(x => x.PkceKey).IsUnique();
            builder.Property(x => x.ClientId).HasMaxLength(100).IsRequired();
            builder.Property(x => x.RedirectUri).HasMaxLength(500);
            builder.Property(x => x.Scope).HasMaxLength(1000);
            builder.Property(x => x.CodeChallenge).HasMaxLength(200);
            builder.Property(x => x.CodeChallengeMethod).HasMaxLength(10);
            builder.Property(x => x.State).HasMaxLength(500);
            builder.Property(x => x.Nonce).HasMaxLength(500);
            builder.Property(x => x.ExpiresAt).HasColumnType("datetimeoffset").IsRequired();
            builder.Property(x => x.CreatedAt).HasColumnType("datetimeoffset").IsRequired();
        }
    }
}
