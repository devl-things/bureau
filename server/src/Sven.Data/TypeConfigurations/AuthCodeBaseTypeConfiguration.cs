using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sven.Data.Models;

namespace Sven.Data.TypeConfigurations
{
    internal sealed class AuthCodeBaseTypeConfiguration : IEntityTypeConfiguration<AuthCodeDb>
    {
        public void Configure(EntityTypeBuilder<AuthCodeDb> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.Code).HasMaxLength(64).IsRequired();
            builder.HasIndex(x => x.Code).IsUnique();

            builder.Property(x => x.ClientId).HasMaxLength(100).IsRequired();
            builder.Property(x => x.RedirectUri).HasMaxLength(500).IsRequired();
            builder.Property(x => x.Scope).HasMaxLength(1000).IsRequired();
            builder.Property(x => x.Claims).IsRequired();

            builder.Property(x => x.CodeChallenge).HasMaxLength(200);
            builder.Property(x => x.CodeChallengeMethod).HasMaxLength(10);
            builder.Property(x => x.Nonce).HasMaxLength(500);
        }
    }
}
