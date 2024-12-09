using Bureau.UI.Constants;
using Bureau.UI.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace Bureau.UI.Components.DevLMultiSelect
{
    public class DevLMultiSelectEventArgs<T> : DevLEventArgs  where T : ISelection
    {
        public T ChangedItem { get; private set; } = default!;
        public List<T> Items { get; private set; } = new List<T>();

        public DevLMultiSelectEventArgs(List<T>? items)
        {
            if (items != null && items.Count > 0)
            {
                Items = items;
            }
        }

        internal bool TryAction(EventAction action, T selection) 
        {
            switch (action)
            {
                case EventAction.Remove:
                    return TryRemove(selection);
                case EventAction.Add:
                default:
                    return TryAdd(selection);
            }
        }

        private bool TryAdd(T selection) 
        {
            if (Items.Contains(selection))
            {
                return false;
            }
            Items.Add(selection);
            ChangedItem = selection;
            Action = EventAction.Add;
            return true;
        }

        private bool TryRemove(T selection) 
        {
            if (Items.Remove(selection))
            {
                ChangedItem = selection;
                Action = EventAction.Remove;
                return true;
            }
            return false;
        }
    }
}
