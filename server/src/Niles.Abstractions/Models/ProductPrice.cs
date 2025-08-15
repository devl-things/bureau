namespace Niles.Models
{
    public class ProductPrice
    {
        public Product Product { get; set; } = null!;
        public Store Store { get; set; } = null!;
        public DateOnly Date { get; set; }
        public decimal Price { get; set; }
    }
}
