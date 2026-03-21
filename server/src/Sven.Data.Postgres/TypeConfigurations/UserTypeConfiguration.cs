using Bureau.EntityFrameworkCore.TypeConfigurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sven.Data.Models;

namespace Sven.Data.Postgres.TypeConfigurations
{
    internal class UserTypeConfiguration : AuditTypeConfiguration<UserDb>
    {
        public override void Configure(EntityTypeBuilder<UserDb> builder)
        {
            base.Configure(builder);

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).ValueGeneratedOnAdd();
            builder.Property(x => x.Identifier).HasMaxLength(100).IsRequired();
            builder.Property(x => x.Username).HasMaxLength(100).IsRequired();
            builder.Property(x => x.PasswordHash).HasMaxLength(100);
            builder.Property(x => x.DisplayName).HasMaxLength(200);

            builder.HasIndex(x => x.Identifier).IsUnique();
            builder.HasIndex(x => x.Username).IsUnique();

            builder.ConfigureAuditFields();
        }
    }
}
