using Bureau.Core;
using Sven.Models;

namespace Sven.Data
{
    public interface IUserStore
    {
        Task<Result<SvenUser>> GetByUsernameAsync(string username, CancellationToken cancellationToken);
        Task<Result> StoreAsync(SvenUser user, CancellationToken cancellationToken);
    }
}
