using System.Text;

namespace Bureau.Models
{
    public struct QuantityDetails
    {
        public decimal? Quantity { get; set; }
        public string? Unit { get; set; }

        public bool IsEmpty()
        {
            return !Quantity.HasValue && string.IsNullOrWhiteSpace(Unit);
        }
        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (Quantity.HasValue)
            {
                sb.Append(Quantity);
            }
            if (!string.IsNullOrWhiteSpace(Unit))
            {
                if (sb.Length > 0)
                {
                    sb.Append(' ');
                }
                sb.Append(Unit);
            }
            return sb.ToString();
        }
    }
}
