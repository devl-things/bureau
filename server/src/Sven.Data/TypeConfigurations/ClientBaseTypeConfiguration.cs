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
            builder.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();
            builder.Property(x => x.Type)
                .HasMaxLength(15)
                .IsRequired();

            ConfigureJsonConversion(builder.Property(e => e.ClientAddendum));
            ConfigureJsonConversion(builder.Property(e => e.Contacts));
            ConfigureJsonConversion(builder.Property(e => e.RedirectUris));
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
