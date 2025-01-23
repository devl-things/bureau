using Bureau.Core;

namespace Bureau.Providers
{
    //TODO [maybe] can go in Core
    public interface IDtoProvider<T>
    {
        Task<Result<T>> GetByIdAsync(string id, CancellationToken cancellationToken);
        Task<PaginatedResult<List<T>>> GetAsync(int? page, int? limit, CancellationToken cancellationToken);
    }
}
