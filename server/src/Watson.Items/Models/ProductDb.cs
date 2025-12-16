namespace Niles.Data.Models
{
    public sealed class ProductDb
    {
        public Guid Id { get; set; }
        public string CanonicalName { get; set; } = string.Empty;
        public string? Brand { get; set; }
        public string? Unit { get; set; }
        public decimal? NetQuantity { get; set; }
    }
}
