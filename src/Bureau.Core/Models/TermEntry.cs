using Bureau.Core.Extensions;

namespace Bureau.Core.Models
{
    public class TermEntry : BaseEntry, ITag
    {
        public string Title { get; private set; } = string.Empty;

        public string Label { get; private set; } = string.Empty;

        public TermEntry(string id)
        {
            Id = id;
        }
        public TermEntry(string id, string title) : this(id)
        {
            SetTitle(title);
        }

        public void SetTitle(string title)
        {
            Title = title;
            Label = title.ToKebabCase();
        }

        public static string GetLabel(string title)
        {
            return title.ToKebabCase();
        }
    }
}
