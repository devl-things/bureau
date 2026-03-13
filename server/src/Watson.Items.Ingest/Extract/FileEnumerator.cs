namespace Niles.Etl.Extract
{
    public sealed class FileEnumerator : IFileEnumerator
    {
        public IEnumerable<string> EnumerateCsv(string folder)
        {
            return Directory.EnumerateFiles(folder, "*.csv", SearchOption.TopDirectoryOnly)
                            .OrderBy(p => p, StringComparer.OrdinalIgnoreCase);
        }
    }
}
