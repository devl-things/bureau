namespace Niles.Etl.Jobs
{
    public interface IProcessedStore
    {
        bool IsProcessed(JobType jobType, string key, string? scope = null);
        bool MarkProcessed(JobType jobType, string key, string? scope = null);
    }
}
