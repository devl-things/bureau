namespace Bureau.UI.Models
{
    //TODO zasto imamo base i page
    public abstract class BasePageAddress
    {
        public virtual string Url { get; }
        public abstract Dictionary<string, object?> QueryParameters { get; }

        protected BasePageAddress(string url)
        {
            Url = url;
        }
    }

    public sealed class PageAddress : BasePageAddress
    {
        public PageAddress(string url, Dictionary<string, object?> parameters) : base(url)
        {
            QueryParameters = parameters;
        }
        public override Dictionary<string, object?> QueryParameters { get; }
    }
}