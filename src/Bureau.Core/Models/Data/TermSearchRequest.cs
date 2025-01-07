using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Core.Models.Data
{
    public class TermSearchRequest
    {
        public HashSet<string> Terms { get; set; } = default!;

        public TermRequestType RequestType { get; set; } = TermRequestType.Label;
    }
}
