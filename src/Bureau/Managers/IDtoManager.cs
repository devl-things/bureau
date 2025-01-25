using Bureau.Core;

namespace Bureau.Managers
{
    //TODO [maybe] can go in Core
    public interface IDtoManager<TId, TDto>
    {
        Task<Result> DeleteAsync(TId id, CancellationToken cancellationToken);
        Task<Result<TDto>> InsertAsync(TDto dto, CancellationToken cancellationToken);
        Task<Result<TDto>> UpdateAsync(TDto dto, CancellationToken cancellationToken);
    }
}
