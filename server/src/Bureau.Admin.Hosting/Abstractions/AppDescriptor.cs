namespace Bureau.Admin.Hosting
{
    public sealed class AppDescriptor
    {
        public string Key { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// Absolute URL to the admin entry point (e.g. https://chores.domain/admin).
        /// </summary>
        public Uri Url { get; set; } = null!;

        /// <summary>
        /// Optional icon identifier (frame decides how to render it).
        /// </summary>
        public string? Icon { get; set; }
    }
}
