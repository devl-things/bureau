using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Core.Models
{
    public class ExpenseRecord : BaseRecord
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

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
