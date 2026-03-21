using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sven.Data.Models;

namespace Sven.Data.TypeConfigurations
{
    public class UserBaseTypeConfiguration<TEntity> : AuditTypeConfiguration<TEntity> where TEntity : UserDb
    {
        public override void Configure(EntityTypeBuilder<TEntity> builder)
        {
            base.Configure(builder);

            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id)
                .ValueGeneratedOnAdd();
            builder.Property(x => x.Identifier)
                .HasMaxLength(100)
                .IsRequired();
            builder.Property(x => x.Username)
                .HasMaxLength(100)
                .IsRequired();
            builder.Property(x => x.PasswordHash)
                .HasMaxLength(100);
            builder.Property(x => x.DisplayName)
                .HasMaxLength(200);

            builder.HasIndex(x => x.Identifier).IsUnique();
            builder.HasIndex(x => x.Username).IsUnique();
        }
    }
}
