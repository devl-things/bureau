namespace Bureau.Primitives
{
    public interface IHealthProbe
    {
        Task<bool> IsHealthyAsync(CancellationToken cancellationToken = default);
    }
}
