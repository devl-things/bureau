using Bureau.Core;

namespace Bureau.Managers
{
    //TODO [maybe] can go in Core
    public interface IDtoManager<T>
    {
        Task<Result> DeleteAsync(string id, CancellationToken cancellationToken);
        Task<Result<T>> InsertAsync(T calendarDto, CancellationToken cancellationToken);
        Task<Result<T>> UpdateAsync(T calendarDto, CancellationToken cancellationToken);
    }
}
