using Bureau.Core.Models;
using Bureau.Core;

namespace Bureau.Handlers
{
    internal interface ICommandHandler<TId, TDto>
    {
        Task<Result> DeleteAsync(TId id, CancellationToken cancellationToken);
        Task<Result<IReference>> InsertAsync(TDto dto, CancellationToken cancellationToken);
        Task<Result<IReference>> UpdateAsync(TDto dto, CancellationToken cancellationToken);
    }
}
