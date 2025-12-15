namespace Niles.Etl.Extract
{
    public interface IFileEnumerator
    {
        IEnumerable<string> EnumerateCsv(string folder);
    }
}
