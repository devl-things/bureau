using Bureau;
using Sven.Models;

namespace Sven.Data
{
    public interface IUserStore
    {
        Task<bool> ExistsWithEmail(string email, CancellationToken cancellationToken);
        Task<Result<SvenUser>> GetByIdentifierAsync(string userId, CancellationToken cancellationToken);
        Task<Result<SvenUser>> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<Result> RemoveAsync(string userId, CancellationToken cancellationToken);
        Task<Result> StoreAsync(SvenUser user, CancellationToken cancellationToken = default);
    }
}
