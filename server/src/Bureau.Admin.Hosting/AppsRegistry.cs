using Microsoft.Extensions.Options;

namespace Bureau.Admin.Hosting
{
    public sealed class AppsRegistry : IAppsRegistry
    {
        private readonly IReadOnlyList<AppDescriptor> _apps;

        public AppsRegistry(IOptions<AppsRegistryOptions> options)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            AppsRegistryOptions value = options.Value;

            CurrentAppKey = value.CurrentApp ?? string.Empty;
            _apps = value.Apps ?? new List<AppDescriptor>();
        }

        public string CurrentAppKey { get; }

        public IReadOnlyList<AppDescriptor> Apps => _apps;
    }
}
