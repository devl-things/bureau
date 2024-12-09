using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.UI.Components.DevLMultiSelect
{
    public interface ISelection
    {
        public string Name { get; }

        public bool IsNew { get; }
    }
}
