namespace Niles.Chores.Services
{
    public interface IChoresHealthService
    {
        Task<bool> IsHealthyAsync(CancellationToken cancellationToken);
    }
}
