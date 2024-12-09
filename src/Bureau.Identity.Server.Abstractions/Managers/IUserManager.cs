using Bureau.Core;
using Bureau.Identity.Models;
using System.Security.Claims;

namespace Bureau.Identity.Managers
{
    public interface IUserManager
    {
        public Task<Result<BureauUser>> GetUserByIdAsync(IUserId userId, CancellationToken cancellationToken = default);
        public Task<Result<IUserId>> GetUserIdByNameAsync(string userName, CancellationToken cancellationToken = default);

        public Task<Result<IUserId>> GetUserIdByEmailAsync(string email, CancellationToken cancellationToken = default);

        public Task<Result<BureauUser>> AddUserAsync(string userName, string? email, CancellationToken cancellationToken = default);
        public Task<Result<BureauUser>> AddUserAsync(string userName, string? email, string password, CancellationToken cancellationToken = default);

        public Task<Result> RemoveUserAsync(IUserId userId, CancellationToken cancellationToken = default);

        public Task<Result<List<BureauUserLoginModel>>> GetUserLoginsAsync(IUserId userId, CancellationToken cancellationToken = default);

        public Task<Result> RemoveUserLoginAsync(IUserId user, IBureauLoginModel login, CancellationToken cancellationToken = default);

        public Task<Result> UpdateUserLoginTokensAsync(UpdateUserLoginTokensRequest request, CancellationToken cancellationToken = default);

        public Task<Result<Dictionary<string, string?>>> GetUserLoginTokensAsync(GetUserLoginTokensRequest request, CancellationToken cancellationToken = default);

        public Task<Result> AddUserLoginAsync(IUserId user, IBureauLoginModel login, CancellationToken cancellationToken = default);
    }
}
