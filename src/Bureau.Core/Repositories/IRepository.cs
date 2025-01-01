using Bureau.Core.Models;
using Bureau.Core.Models.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bureau.Core.Repositories
{
    public interface IRepository
    {
        Task<Result<AggregateModel>> FetchRecordsAsync(IdSearchRequest idSearchRequest, CancellationToken cancellationToken);
        Task<Result<BaseAggregateModel>> FetchRecordsAsync(EdgeTypeSearchRequest edgeTypeSearchRequest, CancellationToken cancellationToken);
        Task<Result<Dictionary<string, TermEntry>>> FetchTermRecordsAsync(TermSearchRequest termSearchRequest, CancellationToken cancellationToken);
        Task<Result<IReference>> InsertAggregateAsync(AggregateModel insertRequest, CancellationToken cancellationToken);
        Task<Result<IReference>> UpdateAggregateAsync(ExtendedAggregateModel value, CancellationToken cancellationToken);
    }
}
