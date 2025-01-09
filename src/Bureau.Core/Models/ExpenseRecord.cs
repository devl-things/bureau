namespace Bureau.Core.Models
{
    public class ExpenseRecord : BaseRecord
    {
        public IEntryReference Item { get; set; } = default!;

        /// <summary>
        /// Given value
        /// </summary>
        public decimal AmountGross { get; set; }
        /// <summary>
        /// Real value, price
        /// </summary>
        public decimal AmountNet { get; set; }
        public DateTime Date { get; set; }

    }
}
