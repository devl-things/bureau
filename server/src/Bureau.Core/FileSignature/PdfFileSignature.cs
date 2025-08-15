namespace Bureau.Core.FileSignature
{
    public sealed class PdfFileSignature : IFileSignature
    {
        // "%PDF-" (25 50 44 46 2D)
        public bool IsMatch(ReadOnlySpan<byte> b) =>
            b.Length >= 5 && b[0] == 0x25 && b[1] == 0x50 && b[2] == 0x44 && b[3] == 0x46 && b[4] == 0x2D;

        public IEnumerable<string> Extensions => new[] { "pdf" };
    }
}
