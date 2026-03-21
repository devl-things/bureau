using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sven.Data.Models;

namespace Sven.Data.TypeConfigurations
{
    internal sealed class UserVerificationCodeBaseTypeConfiguration : IEntityTypeConfiguration<UserVerificationCodeDb>
    {
        public void Configure(EntityTypeBuilder<UserVerificationCodeDb> builder)
        {
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasMaxLength(36).IsRequired();

            builder.Property(x => x.Email).HasMaxLength(256).IsRequired();
            builder.Property(x => x.VerificationCode).HasMaxLength(10).IsRequired();
            builder.Property(x => x.UserId).HasMaxLength(100);
            builder.Property(x => x.Status).IsRequired();
        }
    }
}
