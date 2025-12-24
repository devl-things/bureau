namespace Bureau.Admin.Hosting
{
    public sealed class AppsRegistryOptions
    {
        public const string SectionName = "Admin";

        /// <summary>
        /// The current host's app key (used to highlight active item in the frame).
        /// Example: "chores".
        /// </summary>
        public string CurrentApp { get; set; } = string.Empty;

        /// <summary>
        /// Global list of admin applications for navigation.
        /// </summary>
        public List<AppDescriptor> Apps { get; set; } = new List<AppDescriptor>();
    }
}
