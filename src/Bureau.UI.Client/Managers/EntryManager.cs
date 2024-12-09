using Bureau.UI.Client.Models;
using Microsoft.AspNetCore.Components.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.UI.Client.Managers
{
    internal class EntryManager : IEntryManager
    {
        public EntryManager() 
        {
        }
        public Task<EntryUI> GetEntryByIdAsync(string id, CancellationToken cancellationToken)
        {
            return Task.FromResult(new EntryUI());
        }

        public Task<EntryUI> GetOrAddDraftEntryAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(new EntryUI()
            {
                Tags = new List<TagUI>()
                {
                    new TagUI("cost", "1")
                }
            });
        }
    }
}
