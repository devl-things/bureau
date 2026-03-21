using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sven.Data.Models;

namespace Sven.Data.TypeConfigurations
{
    internal sealed class FailedExchangeAttemptBaseTypeConfiguration : IEntityTypeConfiguration<FailedExchangeAttemptDb>
    {
        public void Configure(EntityTypeBuilder<FailedExchangeAttemptDb> builder)
        {
            builder.HasKey(x => new { x.ClientId, x.UserId });

            builder.Property(x => x.ClientId).HasMaxLength(100).IsRequired();
            builder.Property(x => x.UserId).HasMaxLength(100).IsRequired();
            builder.Property(x => x.FailureCount).IsRequired();
            builder.Property(x => x.LockoutStartedAt);
        }
    }
}
