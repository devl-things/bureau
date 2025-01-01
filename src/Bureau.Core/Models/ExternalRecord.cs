using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Core.Models
{
    public class ExternalRecord<T> : FlexibleRecord<T>
    {
        public DateTime LastSync { get; set; }
        public bool Changed { get; set; }

        public ExternalRecord(string id) : base(id)
        {
        }
    }
}
