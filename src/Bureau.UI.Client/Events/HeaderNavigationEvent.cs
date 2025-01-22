using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.UI.Client.Events
{
    internal class HeaderNavigationEvent : EventArgs
    {
        private static HeaderNavigationEvent _instance = new HeaderNavigationEvent();
        public static HeaderNavigationEvent Instance 
        { 
            get 
            {
                return _instance;
            } 
        }
    }
}
