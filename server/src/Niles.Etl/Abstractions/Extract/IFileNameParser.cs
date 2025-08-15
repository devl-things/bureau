using Niles.Models;

namespace Niles.Etl.Abstractions.Extract
{
    public interface IFileNameParser
    {
        // Parse store name/address/city/postal + price DateOnly from filename
        (Store store, DateOnly date) Parse(string retailerName, string filePath);
    }
}
