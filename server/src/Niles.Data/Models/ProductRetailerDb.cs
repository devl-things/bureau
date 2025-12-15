namespace Niles.Data.Models
{
    public sealed class ProductRetailerDb
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public ProductDb Product { get; set; } = null!;
        public Guid RetailerId { get; set; }
        public RetailerDb Retailer { get; set; } = null!;
        public string? Code { get; set; }
        public string? Barcode { get; set; }
    }
}
