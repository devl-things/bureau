using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Niles.Chores.Models;

namespace Niles.Chores.TypeConfigurations
{
    internal class CompletedChoreTypeConfiguration : IEntityTypeConfiguration<CompletedChoreDb>
    {
        public void Configure(EntityTypeBuilder<CompletedChoreDb> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.ChoreId).IsRequired();
            builder.Property(x => x.HousekeepingId).IsRequired();

            builder.HasOne(x => x.Chore)
                .WithMany(x => x.CompletedChores)
                .HasForeignKey(x => x.ChoreId);

            builder.HasOne(x => x.Housekeeping)
                .WithMany(h => h.CompletedChores)
                .HasForeignKey(x => x.HousekeepingId);
        }
    }
}
