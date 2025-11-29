namespace Niles.Chores.Abstractions.Services
{
    public interface IChoresHealthService
    {
        Task<bool> IsHealthyAsync(CancellationToken cancellationToken);
    }
}
