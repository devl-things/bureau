namespace Niles.Data.Models
{
    public sealed class ProductPriceDb
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public ProductDb Product { get; set; } = null!;
        public Guid StoreId { get; set; }
        public StoreDb Store { get; set; } = null!;
        public DateOnly PriceDate { get; set; }
        public decimal Price { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? PromoPrice { get; set; }
        public decimal? Lowest30 { get; set; }
    }
}
