using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.UI.Abstractions.Models
{
    public class DynamicComponentModel
    {
        public Type ComponentType { get; set; } = default!;
        public Dictionary<string, object> Parameters { get; set; } = new();
    }
}
