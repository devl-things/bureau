using Bureau.Core;
namespace Bureau.UI.Accessors
{
    public interface IUserAccessor
    {
        public Task<IUserId> GetUserIdAsync(CancellationToken cancellationToken = default);
    }
}
