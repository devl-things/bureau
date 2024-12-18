using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Core.Models
{
    public class FlexibleRecord<T> : BaseRecord
    {
        public string Type { get; set; } = default!;
        public T Data { get; set; } = default!;
    }
}
