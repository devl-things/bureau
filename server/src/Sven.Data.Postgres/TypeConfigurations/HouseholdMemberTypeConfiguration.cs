using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sven.Data.Models;

namespace Sven.Data.Postgres.TypeConfigurations
{
    internal class HouseholdMemberTypeConfiguration : IEntityTypeConfiguration<HouseholdMemberDb>
    {
        public void Configure(EntityTypeBuilder<HouseholdMemberDb> builder)
        {
            builder.ToTable("HouseholdMembers");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.HouseholdIdentifier).HasMaxLength(100).IsRequired();
            builder.Property(x => x.UserId).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Role).HasMaxLength(20).IsRequired();
            builder.Property(x => x.JoinedAt).HasColumnType("timestamptz").IsRequired();
            builder.HasIndex(x => new { x.HouseholdIdentifier, x.UserId }).IsUnique();
        }
    }
}
