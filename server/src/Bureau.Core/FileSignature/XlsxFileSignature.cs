namespace Bureau.Core.FileSignature
{
    public sealed class XlsxFileSignature : IFileSignature
    {
        public bool IsMatch(ReadOnlySpan<byte> b)
        {
            // must be a ZIP
            if (!new ZipFileSignature().IsMatch(b)) return false;

            using var ms = new MemoryStream(b.ToArray(), writable: false);
            using var zip = new System.IO.Compression.ZipArchive(ms, System.IO.Compression.ZipArchiveMode.Read, leaveOpen: true);
            // xlsx has /xl/workbook.xml
            return zip.GetEntry("xl/workbook.xml") != null;
        }

        public IEnumerable<string> Extensions => new[] { "xlsx" };
    }
}
