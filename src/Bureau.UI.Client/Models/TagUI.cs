using Bureau.UI.Components.DevLMultiSelect;

namespace Bureau.UI.Client.Models
{
    public class TagUI : ISelection
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; private set; }

        public bool IsNew 
        { 
            get 
            {
                return string.IsNullOrWhiteSpace(Id);
            } 
        }
        public TagUI(string name, string id)
        {
            Name = name;
            Id = id;
        }
        public TagUI(string name):this(name, string.Empty)
        {
        }
    }
}
