using Bureau.UI.Constants;

namespace Bureau.UI.Models
{
    public class DevLEventArgs :EventArgs
    {
        public EventAction Action { get; protected set; }
    }
}
