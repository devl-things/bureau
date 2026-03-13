namespace Niles.Data
{
    public interface IUnitOfWork : IAsyncDisposable
    {
        Task CommitAsync(CancellationToken ct);
        Task RollbackAsync(CancellationToken ct);
    }
}
