namespace Bureau.Core.Accessors
{
    public interface IUserAccessor
    {
        public Task<IUserId> GetUserIdAsync(CancellationToken cancellationToken = default);
    }
}
