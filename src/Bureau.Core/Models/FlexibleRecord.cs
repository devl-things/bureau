using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Core.Models
{
    public class FlexibleRecord<T> : BaseRecord
    {
        public T Data { get; set; } = default!;
        public FlexibleRecord(string id)
        {
            Id = id;
            Status = RecordStatus.Active;
        }
    }
}
