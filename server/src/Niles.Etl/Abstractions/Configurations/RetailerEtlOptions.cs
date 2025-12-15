using Bureau.Core.Extensions;

namespace Niles.Etl.Configurations
{
    public sealed class RetailerEtlOptions
    {
        public string Retailer { get; set; } = string.Empty;   // key/name
        public string BaseUrl { get; set; } = string.Empty;
        public string Scope { get { return BaseUrl.ToScope(); } } // derived from BaseUrl
        public string ExtractUrl { get; set; } = string.Empty;
        public string InboundFolder { get; set; } = "temp";
        public string ArchiveFolder { get; set; } = "archive";
        public string ErrorFolder { get; set; } = "error";
        public string Encoding { get; set; } = "windows-1250";
        public bool Force { get; set; } = false;
        public bool Recurse { get; set; } = false;
        public int MaxConcurrentItems { get; set; } = 4;

        /// <summary>
        /// Merge runtime job arguments into the options.
        /// CLI/job args override config file values.
        /// </summary>
        public RetailerEtlOptions BindArgs(Dictionary<string, string>? args)
        {
            if (args == null) return this;

            if (args.TryGetValue(nameof(BaseUrl), out string? baseUrl)) BaseUrl = baseUrl;
            if (args.TryGetValue(nameof(ExtractUrl), out string? extractUrl)) ExtractUrl = extractUrl;
            if (args.TryGetValue(nameof(InboundFolder), out string? inbound)) InboundFolder = inbound;
            if (args.TryGetValue(nameof(ArchiveFolder), out string? archive)) ArchiveFolder = archive;
            if (args.TryGetValue(nameof(ErrorFolder), out string? error)) ErrorFolder = error;
            if (args.TryGetValue(nameof(Encoding), out string? enc)) Encoding = enc;
            if (args.TryGetInt(nameof(MaxConcurrentItems), out int maxItems) && maxItems < 100) MaxConcurrentItems = maxItems;
            if (args.TryGetBool(nameof(Force), out bool force)) Force = force;
            if (args.TryGetBool(nameof(Recurse), out bool recurse)) Force = recurse;
            return this;
        }
    }
}
