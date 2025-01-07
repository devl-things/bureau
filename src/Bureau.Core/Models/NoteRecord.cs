using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Core.Models
{
    [Obsolete("Use NoteDetails instead")]
    public class NoteRecord : BaseRecord
    {
        public string Note { get; set; } = default!;
        public NoteRecord(string id)
        {
            Id = id;
        }
    }
}
