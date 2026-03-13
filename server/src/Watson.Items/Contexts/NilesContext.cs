using Microsoft.EntityFrameworkCore;
using Niles.Data.Models;

namespace Niles.Data.Contexts
{
    public sealed class NilesContext : DbContext
    {
        public NilesContext(DbContextOptions<NilesContext> options) : base(options) { }

        public DbSet<RetailerDb> Retailers { get; set; } = null!;
        public DbSet<StoreDb> Stores { get; set; } = null!;
        public DbSet<ProductDb> Products { get; set; } = null!;
        public DbSet<ProductRetailerDb> ProductRetailers { get; set; } = null!;
        public DbSet<ProductStoreDb> ProductShops { get; set; } = null!;
        public DbSet<ProductPriceDb> ProductPrices { get; set; } = null!;
        public DbSet<ImportFileDb> ImportFiles { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<RetailerDb>()
                .HasIndex(x => x.Name).IsUnique();

            modelBuilder.Entity<StoreDb>()
                .HasIndex(x => new { x.RetailerId, x.Name }).IsUnique();

            modelBuilder.Entity<ProductDb>()
                .HasIndex(x => x.CanonicalName).IsUnique();

            modelBuilder.Entity<ProductRetailerDb>()
                .HasIndex(x => new { x.ProductId, x.RetailerId }).IsUnique();

            modelBuilder.Entity<ProductPriceDb>()
                .HasIndex(x => new { x.ProductId, x.StoreId, x.PriceDate }).IsUnique();

            modelBuilder.Entity<ImportFileDb>()
                .HasIndex(x => x.Sha256).IsUnique();
        }
    }
}
