using Bureau;
using Sven;

namespace Sven.Data.Repositories
{
    internal interface IUserRepository
    {
        Task<bool> ExistsWithEmail(string email, CancellationToken cancellationToken);
        Task<Result<SvenUser>> GetByIdentifierAsync(string userId, CancellationToken cancellationToken);
        Task<Result<SvenUser>> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
        Task<Result> RemoveAsync(string userId, CancellationToken cancellationToken);
        Task<Result> StoreAsync(SvenUser user, CancellationToken cancellationToken = default);
    }
}
