namespace Niles.Etl.Jobs
{
    public sealed class StartJobRequest
    {
        public Dictionary<string, string>? Args { get; init; }
    }
}
