using Microsoft.EntityFrameworkCore;
using Sven.Data.Contexts;
using Sven.Data.Models;

namespace Sven.Data.SqlServer
{
    internal class SvenContextSqlServer : SvenContext
    {
        public SvenContextSqlServer(DbContextOptions<SvenContextSqlServer> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);


            //modelBuilder.Entity<ClientDb>()
            //    .Property(e => e.Scope)
            //    .HasColumnType("nvarchar(max)");

            modelBuilder.Entity<ClientDb>()
                .Property(e => e.RefreshTokenLifetime)
                .HasColumnType("time");
            modelBuilder.Entity<ClientDb>()
                .Property(e => e.IdTokenLifetime)
                .HasColumnType("time");
            modelBuilder.Entity<ClientDb>()
                .Property(e => e.AccessTokenLifetime)
                .HasColumnType("time");

            modelBuilder.Entity<ClientDb>()
                .Property(e => e.CreatedAt)
                .HasColumnType("datetimeoffset");
            modelBuilder.Entity<ClientDb>()
                .Property(e => e.UpdatedAt)
                .HasColumnType("datetimeoffset");

        }
    }
}
