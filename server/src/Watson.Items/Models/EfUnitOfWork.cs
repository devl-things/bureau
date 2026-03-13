using Microsoft.EntityFrameworkCore.Storage;

namespace Niles.Data.Models
{
    public sealed class EfUnitOfWork : IUnitOfWork
    {
        private readonly IDbContextTransaction _tx;
        public EfUnitOfWork(IDbContextTransaction tx) { _tx = tx; }
        public Task CommitAsync(CancellationToken ct) { return _tx.CommitAsync(ct); }
        public Task RollbackAsync(CancellationToken ct) { return _tx.RollbackAsync(ct); }
        public ValueTask DisposeAsync() { return _tx.DisposeAsync(); }
    }
}
