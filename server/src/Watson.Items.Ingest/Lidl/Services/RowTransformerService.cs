using Niles.Etl.Lidl.Models;
using Niles.Etl.Models;
using Niles.Models;
using System.Globalization;

namespace Niles.Etl.Lidl.Services
{
    public sealed class RowTransformerService
    {
        private static readonly CultureInfo Hr = CultureInfo.GetCultureInfo("hr-HR");

        public LineSnapshot Transform(string retailerName, Store storeFromFileName, DateOnly priceDate, CsvRowRaw row)
        {
            Product product = new Product
            {
                CanonicalName = (row.Name ?? string.Empty).Trim(),
                Brand = string.IsNullOrWhiteSpace(row.Brand) ? null : row.Brand!.Trim(),
                Unit = string.IsNullOrWhiteSpace(row.Unit) ? null : row.Unit!.Trim(),
                NetQuantity = TryDec(row.NetQty, out decimal nq) ? nq : null
            };

            ProductRetailerInfo pr = new ProductRetailerInfo
            {
                Code = string.IsNullOrWhiteSpace(row.Code) ? null : row.Code!.Trim(),
                Barcode = string.IsNullOrWhiteSpace(row.Barcode) ? null : CleanBarcode(row.Barcode!)
            };

            PricePoint price = new PricePoint
            {
                Date = priceDate,
                Price = TryDec(row.Price, out decimal p) ? p : 0m,
                UnitPrice = TryDec(row.UnitPrice, out decimal up) ? up : null,
                PromoPrice = TryDec(row.PromoPrice, out decimal promo) ? promo : null,
                Lowest30 = TryDec(row.Lowest30, out decimal l30) ? l30 : null
            };

            LineSnapshot snap = new LineSnapshot
            {
                Retailer = new Retailer { Name = retailerName },
                Store = storeFromFileName,
                Product = product,
                ProductRetailer = pr,
                Price = price
            };
            return snap;
        }

        private static bool TryDec(string? s, out decimal value)
        {
            value = 0m;
            if (string.IsNullOrWhiteSpace(s)) return false;
            string normalized = s.Trim().Replace(" ", string.Empty).Replace(',', '.');
            return decimal.TryParse(normalized, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
        }

        private static string CleanBarcode(string raw)
        {
            string s = raw.Trim().Replace(" ", string.Empty).Replace(".", string.Empty).Replace(",", string.Empty);
            if (System.Text.RegularExpressions.Regex.IsMatch(s, @"^[0-9]+(\.[0-9]+)?E\+\d+$", System.Text.RegularExpressions.RegexOptions.IgnoreCase))
            {
                s = System.Text.RegularExpressions.Regex.Replace(s, @"\D", "");
            }
            return s;
        }
    }
}
