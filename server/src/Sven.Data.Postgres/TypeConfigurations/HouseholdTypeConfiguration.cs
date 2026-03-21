using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sven.Data.Models;

namespace Sven.Data.Postgres.TypeConfigurations
{
    internal class HouseholdTypeConfiguration : IEntityTypeConfiguration<HouseholdDb>
    {
        public void Configure(EntityTypeBuilder<HouseholdDb> builder)
        {
            builder.ToTable("Households");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.Identifier).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Name).HasMaxLength(200).IsRequired();
            builder.Property(x => x.CreatedAt).HasColumnType("timestamptz").IsRequired();
            builder.HasIndex(x => x.Identifier).IsUnique();
        }
    }
}
