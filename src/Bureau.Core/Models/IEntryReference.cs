using Bureau.Core.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Core.Models
{
    public interface IEntryReference : IReference
    {
        public string ProviderName { get; }

        public string? ExternalId { get; }
    }
}
