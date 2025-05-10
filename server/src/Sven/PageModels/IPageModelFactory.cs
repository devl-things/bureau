using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Sven.PageModels
{
    public interface IPageModelFactory<out T>
    {
        public T CreateModel(string? type, PageContext context);
    }
}
