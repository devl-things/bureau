using Niles.Models;

namespace Niles.Etl.Lidl.Models
{
    public sealed class LineSnapshot
    {
        public Retailer Retailer { get; set; } = new Retailer();
        public Store Store { get; set; } = new Store();
        public Product Product { get; set; } = new Product();
        public ProductRetailerInfo ProductRetailer { get; set; } = new ProductRetailerInfo();
        public PricePoint Price { get; set; } = new PricePoint();
    }
}
