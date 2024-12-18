using Bureau.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Core.Repositories
{
    public interface ITermEntryRepository
    {
        Task<Result<TermEntry>> GetAsync(IReference id, CancellationToken cancellationToken = default);
    }
}
