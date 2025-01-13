using Bureau.Core.Models;
using Bureau.Core.Models.Data;

namespace Bureau.Core.Repositories
{
    public interface ITermRepository
    {
        Task<Result<Dictionary<string, TermEntry>>> FetchTermRecordsAsync(TermSearchRequest termSearchRequest, CancellationToken cancellationToken);
    }
}
