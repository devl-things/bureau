namespace Niles.Models
{
    public sealed class Product
    {
        public string CanonicalName { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string? Unit { get; set; }
        public decimal? NetQuantity { get; set; }
    }
}
