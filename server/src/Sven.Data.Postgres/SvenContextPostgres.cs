using Microsoft.EntityFrameworkCore;
using Sven.Data.Contexts;
using Sven.Data.Models;

namespace Sven.Data.Postgres
{
    internal class SvenContextPostgres : SvenContext
    {
        public SvenContextPostgres(DbContextOptions<SvenContextPostgres> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //modelBuilder.Entity<ClientDb>()
            //    .Property(e => e.RedirectUris)
            //    .HasColumnType("json");
            //modelBuilder.Entity<ClientDb>()
            //    .Property(e => e.Scope)
            //    .HasColumnType("json");

            modelBuilder.Entity<ClientDb>()
                .Property(e => e.RefreshTokenLifetime)
                .HasColumnType("interval");
            modelBuilder.Entity<ClientDb>()
                .Property(e => e.IdTokenLifetime)
                .HasColumnType("interval");
            modelBuilder.Entity<ClientDb>()
                .Property(e => e.AccessTokenLifetime)
                .HasColumnType("interval");
        }
    }
}
