using Niles.Models;

namespace Niles.Etl.Extract
{
    public interface IFileNameParser
    {
        public string RetailerName { get; }
        (Store store, DateOnly date) Parse(string filePath);
    }
}
