namespace Bureau.Core.Models
{
    public class LocationRecord : BaseRecord
    {
        // Geographic Coordinates
        public double Latitude { get; set; }
        public double Longitude { get; set; }

        // Address Components
        public string Street { get; set; } = default!;
        public string City { get; set; } = default!;
        public string State { get; set; } = default!;
        public string Country { get; set; } = default!;
        public string PostalCode { get; set; } = default!;

        // Optional Additional Information
        public string Timezone { get; set; } = default!;
        public double? Altitude { get; set; } // Nullable for optional use

        // Full Address Property (Optional for convenience)
        public string FullAddress => $"{Street}, {City}, {State}, {PostalCode}, {Country}";

        public override string ToString()
        {
            return $"Location: {FullAddress} (Lat: {Latitude}, Long: {Longitude})";
        }
    }
}
