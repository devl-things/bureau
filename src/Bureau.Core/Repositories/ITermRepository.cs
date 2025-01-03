using Bureau.Core.Models.Data;
using Bureau.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Core.Repositories
{
    public interface ITermRepository
    {
        Task<Result<Dictionary<string, TermEntry>>> FetchTermRecordsAsync(TermSearchRequest termSearchRequest, CancellationToken cancellationToken);
    }
}
