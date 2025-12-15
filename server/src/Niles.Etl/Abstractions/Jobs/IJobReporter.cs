namespace Niles.Etl.Jobs
{
    public interface IJobReporter
    {
        void Report(string jobId, JobProgress progress);
    }
}
