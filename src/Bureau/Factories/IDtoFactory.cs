using Bureau.Core.Models.Data;
using Bureau.Core;

namespace Bureau.Factories
{
    internal interface IDtoFactory<T>
    {
        public Result<T> Create(InsertAggregateModel aggregate);
        PaginatedResult<List<T>> CreatePaged(QueryAggregateModel aggregate);

    }

    internal interface IPagedDtoFactory<T>
    {
    }
}
