namespace Bureau.UI.Web.Components.Helpers
{
    public class PageDetails
    {
        public string Title { get; init; } = "Title not set";

        public PageDetails()
        {
            
        }
        public PageDetails(string title)
        {
            Title = title;
        }
    }
}
