namespace Niles.Models
{
    public sealed class Store
    {
        public string Name { get; set; } = string.Empty; // e.g., "Supermarket 104"
        public Retailer Retailer { get; set; } = new Retailer();
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? PostalCode { get; set; }
    }
}
