using Bureau.Core;

namespace Bureau.Providers
{
    //TODO [maybe] can go in Core
    public interface IDtoProvider<TId, TDto>
    {
        Task<Result<TDto>> GetByIdAsync(TId id, CancellationToken cancellationToken);
        Task<PaginatedResult<List<TDto>>> GetAsync(TId request ,int? page, int? limit, CancellationToken cancellationToken);
    }
}
