using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Recipes.Models
{
    internal class ChangedEntry<T>
    {
        public T Entry { get; set; }
        public bool IsChanged { get; set; } = false;

        public ChangedEntry(T entry)
        {
            Entry = entry;
        }
    }
}
