namespace Niles.Etl.Jobs
{
    public enum JobType
    {
        LidlEtlPrices,
    }

    public static class JobTypeParser
    {
        public static string GetRetailerKey(this JobType jobType)
        {
            if (jobType == JobType.LidlEtlPrices)
            {
                return Niles.Etl.Configurations.Definitions.Retailers.Lidl;
            }
            return string.Empty;
        }
    }
}
