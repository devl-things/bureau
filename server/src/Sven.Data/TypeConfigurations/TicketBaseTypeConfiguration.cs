using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sven.Data.Models;

namespace Sven.Data.TypeConfigurations
{
    internal sealed class TicketBaseTypeConfiguration : IEntityTypeConfiguration<TicketDb>
    {
        public void Configure(EntityTypeBuilder<TicketDb> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();

            builder.Property(x => x.Ticket).HasMaxLength(10).IsRequired();
            builder.HasIndex(x => x.Ticket).IsUnique();

            builder.Property(x => x.UserId).HasMaxLength(100).IsRequired();
        }
    }
}
