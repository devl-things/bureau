using Bureau.Core;
using Bureau.Core.Models;
using Bureau.Core.Models.Data;

namespace Bureau.Handlers
{
    internal interface IInternalQueryHandler
    {
        Task<Result<InsertAggregateModel>> InternalGetAggregateAsync(IReference id, CancellationToken cancellationToken);
    }
}
