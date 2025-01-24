using Bureau.Core.Models;
using Bureau.Core;

namespace Bureau.Handlers
{
    internal interface ICommandHandler<T>
    {
        Task<Result> DeleteAsync(string id, CancellationToken cancellationToken);
        Task<Result<IReference>> InsertAsync(T dto, CancellationToken cancellationToken);
        Task<Result<IReference>> UpdateAsync(T dto, CancellationToken cancellationToken);
    }
}
