using Microsoft.EntityFrameworkCore;
using Sven.Data.Models;
using System.Text.Json;

namespace Sven.Data.Contexts
{
    internal static class ClientModelBuilderExtension
    {
        public static void ConfigureClient(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ClientDb>()
                .HasKey(x => x.Id);
            modelBuilder.Entity<ClientDb>()
                .Property(x => x.Id)
                .ValueGeneratedOnAdd();
            modelBuilder.Entity<ClientDb>()
                .Property(x => x.Identifier)
                .HasMaxLength(100)
                .IsRequired();
            modelBuilder.Entity<ClientDb>()
                .Property(x => x.Type)
                .HasMaxLength(15)
                .IsRequired();
            modelBuilder.Entity<ClientDb>()
                .Property(e => e.RedirectUris)
                .HasConversion(
                    x => JsonSerializer.Serialize(x, (JsonSerializerOptions?)null), // to db
                    x => JsonSerializer.Deserialize<HashSet<string>>(x, (JsonSerializerOptions?)null)!); //from db
            modelBuilder.Entity<ClientDb>()
                .Property(e => e.Scope)
                .HasConversion(
                    x => JsonSerializer.Serialize(x, (JsonSerializerOptions?)null), // to db
                    x => JsonSerializer.Deserialize<HashSet<string>>(x, (JsonSerializerOptions?)null)!); //from db



            modelBuilder.Entity<ClientDb>()
                .Property(x => x.CreatedAt)
                .IsRequired();
            modelBuilder.Entity<ClientDb>()
                .Property(x => x.CreatedBy)
                .IsRequired();
            modelBuilder.Entity<ClientDb>()
                .Property(x => x.UpdatedAt)
                .IsRequired();
            modelBuilder.Entity<ClientDb>()
                .Property(x => x.UpdatedBy)
                .IsRequired();
        }
    }
}
