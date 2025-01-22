using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.UI.Client
{
    public class BureauUIClientModule : IUIClientModule
    {
        public string Name { get; set; } = nameof(BureauUIClientModule);
    }
}
