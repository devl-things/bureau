namespace Niles.Chores
{
    public interface IChoresHealthService
    {
        Task<bool> IsHealthyAsync(CancellationToken cancellationToken);
    }
}
