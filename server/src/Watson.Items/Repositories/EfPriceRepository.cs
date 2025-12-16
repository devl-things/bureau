using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Niles.Data.Contexts;
using Niles.Data.Models;

namespace Niles.Data.Repositories
{
    public sealed class EfPriceRepository : IPriceRepository
    {
        private readonly NilesContext _db;
        public EfPriceRepository(NilesContext db) { _db = db; }

        public async Task<IUnitOfWork> BeginAsync(CancellationToken ct)
        {
            IDbContextTransaction tx = await _db.Database.BeginTransactionAsync(ct);
            return new EfUnitOfWork(tx);
        }

        public async Task<Guid> UpsertRetailerAsync(string retailerName, CancellationToken ct)
        {
            RetailerDb? r = await _db.Retailers.FirstOrDefaultAsync(x => x.Name == retailerName, ct);
            if (r != null) return r.Id;
            r = new RetailerDb { Id = Guid.NewGuid(), Name = retailerName };
            _db.Retailers.Add(r);
            await _db.SaveChangesAsync(ct);
            return r.Id;
        }

        public async Task<Guid> UpsertStoreAsync(Guid retailerId, string storeName, string? address, string? city, string? postalCode, CancellationToken ct)
        {
            StoreDb? s = await _db.Stores.FirstOrDefaultAsync(x => x.RetailerId == retailerId && x.Name == storeName, ct);
            if (s != null)
            {
                bool changed = s.Address != address || s.City != city || s.PostalCode != postalCode;
                if (changed)
                {
                    s.Address = address; s.City = city; s.PostalCode = postalCode;
                    await _db.SaveChangesAsync(ct);
                }
                return s.Id;
            }
            s = new StoreDb { Id = Guid.NewGuid(), RetailerId = retailerId, Name = storeName, Address = address, City = city, PostalCode = postalCode };
            _db.Stores.Add(s);
            await _db.SaveChangesAsync(ct);
            return s.Id;
        }

        public async Task<Guid> UpsertProductAsync(string canonicalName, string? brand, string? unit, decimal? netQuantity, CancellationToken ct)
        {
            ProductDb? p = await _db.Products.FirstOrDefaultAsync(x => x.CanonicalName == canonicalName, ct);
            if (p != null)
            {
                bool changed = p.Brand != brand || p.Unit != unit || p.NetQuantity != netQuantity;
                if (changed)
                {
                    p.Brand = brand; p.Unit = unit; p.NetQuantity = netQuantity;
                    await _db.SaveChangesAsync(ct);
                }
                return p.Id;
            }
            p = new ProductDb { Id = Guid.NewGuid(), CanonicalName = canonicalName, Brand = brand, Unit = unit, NetQuantity = netQuantity };
            _db.Products.Add(p);
            await _db.SaveChangesAsync(ct);
            return p.Id;
        }

        public async Task UpsertProductRetailerAsync(Guid productId, Guid retailerId, string? sifra, string? barcode, CancellationToken ct)
        {
            ProductRetailerDb? map = await _db.ProductRetailers.FirstOrDefaultAsync(x => x.ProductId == productId && x.RetailerId == retailerId, ct);
            if (map != null)
            {
                bool changed = false;
                if (!string.IsNullOrWhiteSpace(sifra) && map.Code != sifra) { map.Code = sifra; changed = true; }
                if (!string.IsNullOrWhiteSpace(barcode) && map.Barcode != barcode) { map.Barcode = barcode; changed = true; }
                if (changed) await _db.SaveChangesAsync(ct);
                return;
            }
            map = new ProductRetailerDb { Id = Guid.NewGuid(), ProductId = productId, RetailerId = retailerId, Code = sifra, Barcode = barcode };
            _db.ProductRetailers.Add(map);
            await _db.SaveChangesAsync(ct);
        }

        public async Task<bool> UpsertPriceAsync(Guid productId, Guid storeId, DateOnly date, decimal price, decimal? unitPrice, decimal? promoPrice, decimal? lowest30, CancellationToken ct)
        {
            ProductPriceDb? existing = await _db.ProductPrices.FirstOrDefaultAsync(x => x.ProductId == productId && x.StoreId == storeId && x.PriceDate == date, ct);
            if (existing != null)
            {
                bool changed = existing.Price != price || existing.UnitPrice != unitPrice || existing.PromoPrice != promoPrice || existing.Lowest30 != lowest30;
                if (changed)
                {
                    existing.Price = price;
                    existing.UnitPrice = unitPrice;
                    existing.PromoPrice = promoPrice;
                    existing.Lowest30 = lowest30;
                    await _db.SaveChangesAsync(ct);
                }
                return false;
            }

            ProductPriceDb row = new ProductPriceDb
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                StoreId = storeId,
                PriceDate = date,
                Price = price,
                UnitPrice = unitPrice,
                PromoPrice = promoPrice,
                Lowest30 = lowest30
            };
            _db.ProductPrices.Add(row);
            await _db.SaveChangesAsync(ct);
            return true;
        }
    }
}
