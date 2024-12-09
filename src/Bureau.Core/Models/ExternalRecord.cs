using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Core.Models
{
    internal class ExternalRecord<T> : BaseRecord
    {
        public string Type { get; set; } = default!;
        public T Data { get; set; } = default!;

        public DateTime LastSync { get; set; }
        public bool Changed { get; set; }
    }
}
