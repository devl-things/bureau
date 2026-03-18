using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sven.Data.Models;

namespace Sven.Data.SqlServer.TypeConfigurations
{
    internal sealed class TicketTypeConfiguration : IEntityTypeConfiguration<TicketDb>
    {
        public void Configure(EntityTypeBuilder<TicketDb> builder)
        {
            builder.ToTable("Tickets");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.Ticket).HasMaxLength(10).IsRequired();
            builder.HasIndex(x => x.Ticket).IsUnique();
            builder.Property(x => x.UserId).HasMaxLength(100).IsRequired();
            builder.Property(x => x.ExpiresAt).HasColumnType("datetimeoffset").IsRequired();
            builder.Property(x => x.CreatedAt).HasColumnType("datetimeoffset").IsRequired();
        }
    }
}
