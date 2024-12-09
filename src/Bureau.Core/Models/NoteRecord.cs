using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Core.Models
{
    public class NoteRecord : BaseRecord
    {
        public string Note { get; set; } = default!;
    }
}
