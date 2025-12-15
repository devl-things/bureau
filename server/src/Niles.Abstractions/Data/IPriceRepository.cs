namespace Niles.Data
{
    public interface IPriceRepository
    {
        Task<IUnitOfWork> BeginAsync(CancellationToken ct);

        Task<Guid> UpsertRetailerAsync(string retailerName, CancellationToken ct);
        Task<Guid> UpsertStoreAsync(Guid retailerId, string storeName, string? address, string? city, string? postalCode, CancellationToken ct);

        Task<Guid> UpsertProductAsync(string canonicalName, string? brand, string? unit, decimal? netQuantity, CancellationToken ct);
        Task UpsertProductRetailerAsync(Guid productId, Guid retailerId, string? sifra, string? barcode, CancellationToken ct);

        // returns true if inserted, false if updated/no-op
        Task<bool> UpsertPriceAsync(Guid productId, Guid storeId, DateOnly date, decimal price, decimal? unitPrice, decimal? promoPrice, decimal? lowest30, CancellationToken ct);
    }
}
