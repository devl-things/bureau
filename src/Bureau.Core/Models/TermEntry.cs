
using System.Xml.Linq;

namespace Bureau.Core.Models
{
    public class TermEntry : BaseEntry, ITag
    {
        public string Title { get; set; } = string.Empty;

        public string Label { get { return Title; } }
    }
}
