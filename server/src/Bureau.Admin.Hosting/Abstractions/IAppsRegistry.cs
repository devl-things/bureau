namespace Bureau.Admin.Hosting
{
    public interface IAppsRegistry
    {
        string CurrentAppKey { get; }

        IReadOnlyList<AppDescriptor> Apps { get; }
    }
}
