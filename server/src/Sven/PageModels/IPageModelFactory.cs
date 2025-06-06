namespace Sven.PageModels
{
    public interface IPageModelFactory<in C, out T>
    {
        public T CreateModel(string? type, C context);
    }
}
