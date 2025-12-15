namespace Niles.Data.Models
{
    public sealed class ProductStoreDb
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public ProductDb Product { get; set; } = null!;
        public Guid StoreId { get; set; }
        public StoreDb Store { get; set; } = null!;
        public string? StoreSku { get; set; }
    }
}
