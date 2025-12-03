using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Niles.Chores.Models;

namespace Niles.Chores.TypeConfigurations
{
    internal class CriticalChoreTypeConfiguration : AuditTypeConfiguration<CriticalChoreDb>
    {
        public override void Configure(EntityTypeBuilder<CriticalChoreDb> builder)
        {
            base.Configure(builder);

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.ChoreId).IsRequired();
            builder.Property(x => x.Note).HasMaxLength(1000);

            builder.HasOne(x => x.Chore)
                .WithMany(x => x.CriticalChores)
                .HasForeignKey(x => x.ChoreId);

            builder.HasOne(x => x.CompletedChore)
                .WithMany()
                .HasForeignKey(x => x.CompletedChoreId);
        }
    }
}
