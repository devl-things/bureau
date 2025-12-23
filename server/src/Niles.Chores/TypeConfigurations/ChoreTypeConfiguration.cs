using Bureau.EntityFrameworkCore.SqlServer.TypeConfigurations;
using Bureau.EntityFrameworkCore.TypeConfigurations;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Niles.Chores.Models;

namespace Niles.Chores.TypeConfigurations
{
    internal class ChoreTypeConfiguration : AuditTypeConfiguration<ChoreDb>
    {
        public override void Configure(EntityTypeBuilder<ChoreDb> builder)
        {
            base.Configure(builder);
            builder.ConfigureAuditFields();

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.Title).HasMaxLength(200).IsRequired();
            builder.Property(x => x.Description).HasMaxLength(1000);
            builder.Property(x => x.Type).HasConversion<int>().IsRequired();
        }

    }
}
