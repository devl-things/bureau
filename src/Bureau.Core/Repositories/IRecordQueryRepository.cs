using Bureau.Core.Models.Data;

namespace Bureau.Core.Repositories
{
    public interface IRecordQueryRepository<TRequest, TResult> where TResult : QueryAggregateModel
    {
        public Task<Result<TResult>> FetchRecordsAsync(TRequest idSearchRequest, CancellationToken cancellationToken);
    }
}
