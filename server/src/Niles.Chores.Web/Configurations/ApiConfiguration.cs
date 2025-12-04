namespace Niles.Chores.Web.Configurations
{
    public class ApiConfiguration
    {
        public string BaseUrl { get; set; } = string.Empty;

        public string ChoresUrl { get; set; } = "/api/chores";
        public string HealthUrl { get; set; } = "/api/health";
        public string HousekeepingUrl { get; set; } = "/api/housekeeping";
        public string HousekeepingSubmitSegment { get; set; } = "/submit";

        public string ChoresEndpoint { get { return $"{BaseUrl}{ChoresUrl}"; } }
        public string HealthEndpoint { get { return $"{BaseUrl}{HealthUrl}"; } }
        public string HousekeepingEndpoint { get { return $"{BaseUrl}{HousekeepingUrl}"; } }
        public string HousekeepingSubmitEndpoint { get { return $"{HousekeepingEndpoint}{HousekeepingSubmitSegment}"; } }
    }
}
