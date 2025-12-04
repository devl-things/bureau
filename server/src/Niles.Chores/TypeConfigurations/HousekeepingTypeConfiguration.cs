using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Niles.Chores.Models;

namespace Niles.Chores.TypeConfigurations
{
    internal class HousekeepingTypeConfiguration : AuditTypeConfiguration<HousekeepingDb>
    {
        public override void Configure(EntityTypeBuilder<HousekeepingDb> builder)
        {
            base.Configure(builder);

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.Timestamp).HasColumnType("datetimeoffset").IsRequired();
            builder.Property(x => x.Duration).HasColumnType("time").IsRequired();
            builder.Property(x => x.Note).HasMaxLength(1000);

        }

    }
}
