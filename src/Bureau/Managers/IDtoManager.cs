using Bureau.Core;

namespace Bureau.Managers
{
    //TODO [maybe] can go in Core
    public interface IDtoManager<T>
    {
        Task<Result> DeleteAsync(string id, CancellationToken cancellationToken);
        Task<Result<T>> InsertAsync(T dto, CancellationToken cancellationToken);
        Task<Result<T>> UpdateAsync(T dto, CancellationToken cancellationToken);
    }
}
