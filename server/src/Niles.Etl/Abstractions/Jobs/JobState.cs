namespace Niles.Etl.Jobs
{
    public enum JobState
    {
        Queued,
        Running,
        Completed,
        Failed,
        Canceled
    }
}
