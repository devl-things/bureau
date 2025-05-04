using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Sven.Data.Models;
using System.Text.Json;

namespace Sven.Data.TypeConfigurations
{
    public class ClientBaseTypeConfiguration<TEntity> : AuditTypeConfiguration<TEntity> where TEntity : ClientDb
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
            builder.Property(x => x.AuthMethod)
                .HasMaxLength(100);
            builder.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();
            builder.Property(x => x.Type)
                .HasMaxLength(15)
                .IsRequired();

            builder.Property(x => x.ClientUri)
                .HasMaxLength(2000);
            builder.Property(x => x.JwksUri)
                .HasMaxLength(2000);
            builder.Property(x => x.LogoUri)
                .HasMaxLength(2000);
            builder.Property(x => x.PolicyUri)
                .HasMaxLength(2000);
            builder.Property(x => x.TosUri)
                .HasMaxLength(2000);

            ConfigureJsonConversion(builder.Property(e => e.Contacts));
            ConfigureJsonConversion(builder.Property(e => e.GrantTypes));
            ConfigureJsonConversion(builder.Property(e => e.RedirectUris));
            ConfigureJsonConversion(builder.Property(e => e.ResponseTypes));
            ConfigureJsonConversion(builder.Property(e => e.Scope));

            builder.HasIndex(x => x.Identifier).IsUnique();
        }

        public static void ConfigureJsonConversion<T>(PropertyBuilder<T> propertyBuilder)
        {
            propertyBuilder.HasConversion(
                x => JsonSerializer.Serialize(x, (JsonSerializerOptions?)null), // to db
                x => JsonSerializer.Deserialize<T>(x, (JsonSerializerOptions?)null)!); // from db
        }

        public class HashSetComparer<T> : ValueComparer<HashSet<T>>
        {
            public HashSetComparer() : base(
                (a, b) => a != null && b != null && a.SetEquals(b),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v!.GetHashCode())),
                c => new HashSet<T>(c))
            { }
        }
    }
}
