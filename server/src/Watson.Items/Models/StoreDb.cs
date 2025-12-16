namespace Niles.Data.Models
{
    public sealed class StoreDb
    {
        public Guid Id { get; set; }
        public Guid RetailerId { get; set; }
        public RetailerDb Retailer { get; set; } = null!;
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
    }
}
