using Bureau.Core;
using Bureau.UI.Client.Models;
using Bureau.UI.Components.DevLMultiSelect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.UI.Client.Managers
{
    internal class TagManager : ITagManager
    {
        public Result<TagUI> Create(string name)
        {
            return new TagUI(name);
        }

        public Task<List<TagUI>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new List<TagUI>
            {
                new TagUI("Option 1" , "1"),
                new TagUI("Option 2" , "2"),
                new TagUI("Option 3" , "3"),
                new TagUI("Option 4" , "4"),
                new TagUI("Option 5" , "5"),
                new TagUI("Option 6" , "6"),
            });
    }
    }
}
