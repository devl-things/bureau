namespace Niles.Models
{
    public sealed class PricePoint
    {
        public DateOnly Date { get; set; }
        public decimal Price { get; set; }
        public decimal? UnitPrice { get; set; }
        public decimal? PromoPrice { get; set; }
        public decimal? Lowest30 { get; set; }
    }
}
