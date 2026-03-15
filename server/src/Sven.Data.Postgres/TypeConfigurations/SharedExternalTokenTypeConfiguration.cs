using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sven.Data.Models;

namespace Sven.Data.Postgres.TypeConfigurations
{
    internal class SharedExternalTokenTypeConfiguration : IEntityTypeConfiguration<SharedExternalTokenDb>
    {
        public void Configure(EntityTypeBuilder<SharedExternalTokenDb> builder)
        {
            builder.ToTable("SharedExternalTokens");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.Identifier).HasMaxLength(100).IsRequired();
            builder.Property(x => x.HouseholdIdentifier).HasMaxLength(100).IsRequired();
            builder.Property(x => x.OwnerUserId).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Provider).HasMaxLength(50).IsRequired();
            builder.Property(x => x.FeatureKey).HasMaxLength(200).IsRequired();
            builder.Property(x => x.SharedAt).HasColumnType("timestamptz").IsRequired();
            builder.Property(x => x.RevokedAt).HasColumnType("timestamptz");
            builder.HasIndex(x => x.Identifier).IsUnique();
        }
    }
}
