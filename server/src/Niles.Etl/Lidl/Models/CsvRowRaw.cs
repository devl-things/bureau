namespace Niles.Etl.Lidl.Models
{
    public sealed class CsvRowRaw
    {
        public string Name { get; set; } = string.Empty;        // NAZIV
        public string? Brand { get; set; }                       // MARKA
        public string? Unit { get; set; }                        // JEDINICA_MJERE
        public string? NetQty { get; set; }                      // NETO_KOLIČINA
        public string? Code { get; set; }                       // ŠIFRA (retailer-specific)
        public string? Barcode { get; set; }                     // BARKOD (retailer-specific)
        public string? Price { get; set; }                       // MALOPRODAJNA_CIJENA
        public string? UnitPrice { get; set; }                   // CIJENA_ZA_JEDINICU_MJERE
        public string? PromoPrice { get; set; }                  // MPC_ZA_VRIJEME_POSEBNOG_OBLIKA_PRODAJE
        public string? Lowest30 { get; set; }                    // NAJNIZA_CIJENA_U_POSLJ._30_DANA
    }
}
