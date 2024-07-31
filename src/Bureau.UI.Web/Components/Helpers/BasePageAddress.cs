namespace Bureau.UI.Web.Components.Helpers
{
    internal abstract class BasePageAddress
    {
        public virtual string Url { get; }
        internal abstract Dictionary<string, object?> QueryParameters { get; }

        protected BasePageAddress(string url)
        {
            Url = url;
        }
    }

    internal sealed class PageAddress : BasePageAddress
    {
        public PageAddress(string url, Dictionary<string, object?> parameters) : base(url)
        {
            QueryParameters = parameters;
        }
        internal override Dictionary<string, object?> QueryParameters { get; }
    }
}