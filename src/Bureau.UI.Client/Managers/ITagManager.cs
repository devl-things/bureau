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
    public interface ITagManager
    {
        Task<List<TagUI>> GetAllAsync(CancellationToken cancellationToken = default);

        Result<TagUI> Create(string name);
    }
}
